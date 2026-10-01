import { focusableSelector, focusFirstFocusable } from './Focusable.ts';

// Row height for the scroll estimate that brings a derendered row back into the render window. Only used
// while no row is rendered to measure, which is also the only case where the estimate cannot be checked.
const fallbackRowHeightPixels = 32;

// Every cell of this table that can carry the tab stop, header row included. Scoped to the table's own rows
// so content a consumer put in a cell is never mistaken for one.
const tabStopSelector = ':scope > thead > tr > th[data-column-id], :scope > tbody > tr > td[data-column-id], ' +
    ':scope > tbody > tr.no-data > td';

// Where the keyboard is, expressed as what the cells carry rather than as an element: virtualization
// derenders a row and Blazor builds a fresh one in its place, so an element reference would not survive it.
type FocusedCell =
    { readonly kind: 'header'; readonly columnId: string } |
    { readonly kind: 'body'; readonly rowIndex: number; readonly columnId: string } |
    { readonly kind: 'placeholder' };

/**
     The table's one keyboard position, held as DOM focus. The focused cell carries `tabindex="0"` and every
     other cell the constant `-1` .NET renders, so an arrow key moves the `0` and the focus together.
     Movement covers the header row as well; Space selects the focused cell's row, Enter enters a body cell,
     Escape leaves an entered one, and Alt+ArrowDown opens a column's filter.

     The body is a single tab stop because the two boundary elements either side of the scroll container catch
     every arrival by Tab: one from outside is sent on to the focused cell, one from inside is let through and
     the browser carries focus past the table. Nothing inside a cell has its `tabindex` touched to achieve
     that — content a consumer wrote stays as they wrote it and is simply never arrived at, which is also why
     a cell is entered rather than tabbed into.

     Movement, the promotion and the focus calls live here rather than in .NET because under Interactive
     Server every arrow press would otherwise cost a round trip and a body-wide render diff, and an
     auto-repeating arrow would flood the circuit and overshoot the released key. What crosses to .NET is one
     call per selection gesture and one per activation — .NET keeps the anchor, the selection rules and the
     events.

     Rows are addressed by the absolute index their cells carry and columns by their column id, never by
     position: DOM order is pin order, so movement follows column reorder and pinning with no column model on
     this side, and a cell whose row has been scrolled out of the render window can be found again.
     */
export class KeyboardNavigation {
    readonly #container: HTMLElement;
    readonly #tableElement: HTMLElement;
    readonly #dotNetObject: DotNet.DotNetObject;

    // Where the table's tab order begins and ends. Every arrival by Tab crosses one of them, which is what
    // lets the body be a single stop without the table touching the tabindex of anything inside a cell.
    readonly #leadingBoundary: HTMLElement | undefined;
    readonly #trailingBoundary: HTMLElement | undefined;

    // Puts the tab stop back after a render replaced the cell that carried it: virtualization swaps the rows
    // outright and a reshape rebuilds them, and a body with no stop at all is a body Tab cannot reach.
    readonly #mutationObserver: MutationObserver;

    #position: FocusedCell | undefined;

    // The columns either side of the focused header, read while it is still rendered, so hiding its column
    // can fall to a neighbor once the header itself is gone.
    #headerNeighborColumnIds: readonly string[] = [];

    // Raised while the focused row is being brought back into the render window. Virtualization answers a
    // scroll asynchronously, so focus can only return once the row it belongs to has been rendered.
    #awaitingRow = false;

    // Whether the table holds the focus. A row virtualization removes takes focus with it and the browser
    // drops it on the document; that is the one case the table takes it back rather than leaving it.
    #hasFocus = false;

    // Raised only while #reclaimOrphanedFocus moves the focus, which the focusin handler it triggers
    // synchronously has to tell apart from the user moving it.
    #reclaimingFocus = false;

    readonly #keyDownListener = (event: KeyboardEvent): void => {
        const cell = this.#findEventCell(event);

        if (cell === undefined)
            return;

        // A cell is entered exactly while the focus is inside it, and the content of an entered cell owns the
        // keys it gets — bar the one that walks it. Acting on the rest here would run two things off one
        // press and suppressing their defaults would break the control the user is working in.
        if (event.target !== cell) {
            if (isPlainTab(event))
                this.#walkEnteredCell(event, cell);

            return;
        }

        // The body is one stop, so Tab off a cell leaves the whole table rather than stepping into whatever
        // that cell contains.
        if (isPlainTab(event)) {
            this.#leaveTable(event);
            return;
        }

        // Meta combinations are the platform's.
        if (event.metaKey)
            return;

        // Alt combinations are the browser's history navigation, except the one gesture a header cell has
        // that its own .NET key handler does not carry.
        if (event.altKey) {
            if (event.key === 'ArrowDown')
                this.#openColumnFilter(event, cell);

            return;
        }

        switch (event.key) {
            case 'ArrowDown': {
                this.#move(event, cell, 1, 0);
                break;
            }

            case 'ArrowUp': {
                this.#move(event, cell, -1, 0);
                break;
            }

            case 'ArrowRight': {
                this.#move(event, cell, 0, 1);
                break;
            }

            case 'ArrowLeft': {
                this.#move(event, cell, 0, -1);
                break;
            }

            case ' ': {
                this.#selectFocusedRow(event, cell);
                break;
            }

            case 'Enter': {
                // A held key repeats, and every repeat would raise the activation again off the one press.
                if (!event.repeat)
                    this.#enterCell(cell);

                break;
            }

            default: {
                break;
            }
        }
    };

    // Taken on key-up rather than key-down: an editor inside the cell reverts what was typed on its own
    // key-up, and moving the focus out first would take that key-up away from it. Nothing is claimed here
    // either, so the popups, context menus and filter editors that answer Escape first keep answering it.
    readonly #keyUpListener = (event: KeyboardEvent): void => {
        if (event.key !== 'Escape')
            return;

        // A cell is entered exactly while the focus is inside it, so the cell itself holding the focus means
        // there is nothing to leave and the key belongs to whatever is open over the table.
        const cell = this.#findEventCell(event);

        if (cell === undefined || event.target === cell)
            return;

        cell.focus();
    };

    // Focus reaches a cell by Tab, by a click, or because something moved it there, and the tab stop has to
    // come along or the next Tab back in would land somewhere else. This is the direction that yields:
    // it records and promotes but never calls focus() itself, so the two cannot chase each other.
    readonly #focusInListener = (event: FocusEvent): void => {
        this.#hasFocus = true;

        const cell = this.#findEventCell(event);

        if (cell === undefined)
            return;

        this.#adopt(cell);

        // Focus taken back after a render did not scroll, and must not be corrected into doing so.
        if (this.#reclaimingFocus)
            return;

        // The browser scrolls a newly focused element into view knowing nothing of the sticky header or of
        // the pinned columns a cell can sit behind, so the correction runs after it.
        this.#scrollCellIntoView(cell);
    };

    readonly #focusOutListener = (event: FocusEvent): void => {
        const { target, relatedTarget } = event;

        if (relatedTarget instanceof Node && this.#container.contains(relatedTarget))
            return;

        // Virtualization removing the focused row also lands here, and that is not the user leaving: the table
        // takes the focus back once the row is gone. Chromium sends this event just before it removes the
        // cell, so the check has to wait until the removal is done. A microtask is the earliest point that
        // is true: it runs as soon as the code removing the row returns, before the browser handles any
        // further input or timer that could move the focus again. The `MutationObserver` that takes the
        // focus back also runs as a microtask, and either order works, because `#hasFocus` stays `true`
        // until this check clears it.
        queueMicrotask(() => {
            if (target instanceof Node && !target.isConnected)
                return;

            this.#hasFocus = this.#container.contains(document.activeElement);
        });
    };

    // A boundary never keeps the focus. Arriving from outside the table, Tab is coming in and is sent on to
    // the cell it belongs on; arriving from inside, the table put it here on its way out and the browser's
    // own Tab is about to carry it past the table, so it is left alone.
    readonly #boundaryFocusInListener = (event: FocusEvent): void => {
        if (event.relatedTarget instanceof Node && this.#container.contains(event.relatedTarget))
            return;

        const cell = this.#findPositionCell() ?? this.#findDefaultCell();

        cell?.focus();
    };

    readonly #bodyMutated = (): void => {
        this.#restoreTabStop();
    };

    constructor(container: HTMLElement, tableElement: HTMLElement, dotNetObject: DotNet.DotNetObject) {
        this.#container = container;
        this.#tableElement = tableElement;
        this.#dotNetObject = dotNetObject;

        // Bubble phase, not capture: a capture listener would run before the content of a cell and could
        // never be talked out of a key, which is what makes cell content the owner of the keys it handles.
        this.#container.addEventListener('keydown', this.#keyDownListener);
        this.#container.addEventListener('keyup', this.#keyUpListener);
        this.#container.addEventListener('focusin', this.#focusInListener);
        this.#container.addEventListener('focusout', this.#focusOutListener);

        // Siblings of the scroll container rather than children of it: focusing an element inside a scroll
        // box makes the browser scroll to it, and an arrival would jerk the table to the top before the
        // redirect below moved focus anywhere.
        this.#leadingBoundary = findBoundary(container.previousElementSibling ?? undefined);
        this.#trailingBoundary = findBoundary(container.nextElementSibling ?? undefined);

        this.#leadingBoundary?.addEventListener('focusin', this.#boundaryFocusInListener);
        this.#trailingBoundary?.addEventListener('focusin', this.#boundaryFocusInListener);

        this.#mutationObserver = new MutationObserver(this.#bodyMutated);
        this.#mutationObserver.observe(this.#tableElement, { childList: true, subtree: true });

        this.#restoreTabStop();
    }

    // Claims the press whether or not it moves anything: at an edge a deliberate no-op is the answer, and
    // letting the key through would scroll the container out from under the focused cell instead.
    #move(event: KeyboardEvent, cell: HTMLTableCellElement, rowOffset: number, columnOffset: number): void {
        event.preventDefault();

        // A scroll took the focused row out of the render window and the stop is parked on another cell. It
        // is a position the user set on purpose, so it is brought back rather than abandoned; the press that
        // follows steps on from it, because the rows it has to step through only arrive once the server has
        // answered the scroll.
        if (this.#position?.kind === 'body' && this.#findPositionCell() === undefined) {
            this.#scrollToPositionRow();
            return;
        }

        const position = this.#describe(cell);

        if (position === undefined)
            return;

        if (columnOffset !== 0) {
            this.#moveToAdjacentColumn(cell, columnOffset);
            return;
        }

        if (position.kind === 'placeholder') {
            // A table with no rows to navigate still reaches its header row, so sorting and filtering stay
            // available on it.
            if (rowOffset < 0)
                this.#getHeaderCells()[0]?.focus();

            return;
        }

        if (position.kind === 'header') {
            if (rowOffset > 0)
                this.#findFirstBodyCell(position.columnId)?.focus();

            return;
        }

        if (rowOffset < 0 && position.rowIndex === 0) {
            this.#findHeaderCell(position.columnId)?.focus();
            return;
        }

        // A row that is not rendered is the end of the set or the end of the render window, and this does not
        // tell the two apart: the edge of the set has to be a no-op, so both are. Ordinary stepping is
        // unaffected because the overscan keeps rendered rows beyond the visible area, and the scroll each
        // move performs is what makes virtualization produce the next batch.
        //
        // Accepted consequence: a press that outruns those rows — holding the arrow down through a refill —
        // is dropped rather than queued, so a held key travels in bursts and the next press carries on.
        const row = this.#findRow(position.rowIndex + rowOffset);

        this.#findColumnCell(row, position.columnId)?.focus();
    }

    // Sibling walking rather than a column lookup: the table renders its columns in pin order, so the cell
    // next to this one is the next column the user sees, hidden and reordered columns included. One rule for
    // the header row and the body, because a header cell is a sibling of a header cell.
    #moveToAdjacentColumn(cell: HTMLTableCellElement, columnOffset: number): void {
        const sibling = columnOffset < 0 ? cell.previousElementSibling : cell.nextElementSibling;

        if (sibling instanceof HTMLTableCellElement)
            sibling.focus();
    }

    #selectFocusedRow(event: KeyboardEvent, cell: HTMLTableCellElement): void {
        // Claimed unconditionally, whether or not anything comes of it. Space's default on a focused cell is
        // to page the scroll container down, which would throw the focused cell off screen — the one thing
        // every other key here goes out of its way to avoid — and leaving it to the browser only while the
        // table happens to have no selection would make one key mean two things depending on state nobody can
        // see. Scrolling by keyboard stays available through PageUp and PageDown, which this never claims.
        event.preventDefault();

        // A held key repeats, and every repeat would toggle the row again, so the state a press ends in would
        // depend on how long it was held.
        if (event.repeat)
            return;

        // Selection is gated where movement is not, so with the row-click channel switched off there is
        // nothing to ask .NET for and the press ends here.
        if (this.#container.dataset.rowClickSelection === undefined)
            return;

        // A header cell and the no-data placeholder carry no row, which is what makes the press a no-op on
        // them without a case of their own.
        const { rowSequence } = cell.dataset;

        if (rowSequence === undefined)
            return;

        void this.#dotNetObject.invokeMethodAsync('SelectRowAsync', Number(rowSequence), event.ctrlKey, event.shiftKey)
            .catch(() => {/* circuit gone, nothing left to select in */});
    }

    // Enter carries no default worth suppressing, so the press is passed on either way — on a header cell it
    // is the sort gesture .NET answers on key-up. .NET decides whether the column has a handler at all: this
    // side cannot know without a contract naming every wired column, and an unhandled activation costs one
    // round trip on a deliberate single press. Focus lands on the cell's content from .NET's side, once the
    // render that handler caused has produced it.
    #enterCell(cell: HTMLTableCellElement): void {
        const { rowSequence, columnId } = cell.dataset;

        if (rowSequence === undefined || columnId === undefined)
            return;

        void this.#dotNetObject.invokeMethodAsync('ActivateCellAsync', Number(rowSequence), columnId)
            .catch(() => {/* circuit gone, nothing left to activate */});
    }

    // Tab inside an entered cell walks that cell's own content and returns to the cell from either end of it,
    // so one rule covers a cell with a single widget and a cell with several. The walk is driven here rather
    // than left to the browser, which would carry on into the next cell's content instead.
    #walkEnteredCell(event: KeyboardEvent, cell: HTMLTableCellElement): void {
        event.preventDefault();

        const widgets = [...cell.querySelectorAll<HTMLElement>(focusableSelector)];
        const position = event.target instanceof HTMLElement ? widgets.indexOf(event.target) : -1;

        // A press the walk cannot place started on something it steps over — a `tabindex="-1"` control the
        // user clicked. Claimed all the same, so nothing leaves a cell sideways.
        if (position === -1) {
            cell.focus();
            return;
        }

        const next = widgets[position + (event.shiftKey ? -1 : 1)];

        (next ?? cell).focus();
    }

    // Hands focus to the boundary the press is heading for and lets the key run: the browser works its next
    // stop out from where focus is once this returns, which by then is past everything the table holds.
    #leaveTable(event: KeyboardEvent): void {
        const boundary = event.shiftKey ? this.#leadingBoundary : this.#trailingBoundary;

        boundary?.focus();
    }

    // Clicking the trigger is what opens the panel, and the trigger is a real button, so the gesture reaches
    // the same handler a mouse reaches. Only a header cell renders one, which is what keeps this off the body.
    #openColumnFilter(event: KeyboardEvent, cell: HTMLTableCellElement): void {
        const button = cell.querySelector<HTMLElement>('.column-filter-button');

        if (button === null)
            return;

        event.preventDefault();
        button.click();
    }

    // The body carries exactly one tab stop whatever a render did to the cell that held it. Run on every
    // change to the rows and once on attach, which is also what seeds the default cell.
    #restoreTabStop(): void {
        const cell = this.#findPositionCell();

        if (cell === undefined) {
            this.#parkTabStop();
            return;
        }

        this.#adopt(cell);

        if (this.#awaitingRow) {
            this.#awaitingRow = false;

            // The move that asked for this row was made from a parked stop, so the focus is on another cell
            // and has to come along.
            if (this.#hasFocus)
                cell.focus();

            this.#scrollCellIntoView(cell);

            return;
        }

        this.#reclaimOrphanedFocus(cell);
    }

    // The position's cell is not rendered. The body must still have a stop, so one goes on a cell that is.
    #parkTabStop(): void {
        const headerNeighbor = this.#findHeaderNeighbor();
        const fallback = headerNeighbor ?? this.#findDefaultCell();

        if (fallback === undefined)
            return;

        const parkedPosition = this.#findParkedPosition();

        // The neighboring header is where the keyboard now is. The default cell only holds the stop until
        // something names a position: recorded as one, a header seeded before the rows arrived would keep the
        // stop after they had, instead of it moving on to the first row.
        if (headerNeighbor === undefined) {
            this.#position = undefined;
            this.#promote(fallback);
        } else {
            this.#adopt(fallback);
        }

        this.#reclaimOrphanedFocus(fallback);

        if (parkedPosition !== undefined)
            this.#position = parkedPosition;
    }

    // The coordinates the stop parks on behalf of, if any: a row outside the render window is coming back, so
    // it stays the position and the next vertical move scrolls to it. With no rows rendered at all there is
    // nothing left to come back to, and a header whose column was hidden does not come back either.
    #findParkedPosition(): FocusedCell | undefined {
        if (this.#position?.kind !== 'body')
            return undefined;

        if (this.#getRows().length === 0)
            return undefined;

        return this.#position;
    }

    // Takes back the focus a removed row dropped on the document, and only that: focus on anything else is
    // the user's, and pulling it into the table would be a jump nobody asked for.
    #reclaimOrphanedFocus(cell: HTMLTableCellElement): void {
        if (!this.#hasFocus || document.activeElement !== document.body)
            return;

        // The row went because the user is scrolling, so the scroll is theirs. Pulling it back to the cell would
        // undo every wheel turn that makes virtualization render — or rebuild — the rows the focus sat in.
        this.#reclaimingFocus = true;

        try {
            cell.focus({ preventScroll: true });
        } finally {
            this.#reclaimingFocus = false;
        }
    }

    // Records where the keyboard now is and moves the tab stop to it.
    #adopt(cell: HTMLTableCellElement): void {
        const position = this.#describe(cell);

        if (position === undefined)
            return;

        this.#position = position;
        this.#headerNeighborColumnIds = position.kind === 'header' ? readNeighborColumnIds(cell) : [];

        this.#promote(cell);
    }

    // .NET renders a constant `tabindex="-1"` on every cell, so its diff never writes the attribute and this
    // promotion survives any re-render. Demoting reaches only cells, never the content inside one: a widget a
    // consumer put in the tab sequence is theirs to place.
    #promote(cell: HTMLTableCellElement): void {
        for (const promoted of this.#tableElement.querySelectorAll<HTMLElement>(tabStopSelector)) {
            if (promoted !== cell && promoted.getAttribute('tabindex') === '0')
                promoted.setAttribute('tabindex', '-1');
        }

        cell.setAttribute('tabindex', '0');
    }

    #describe(cell: HTMLTableCellElement): FocusedCell | undefined {
        const { columnId } = cell.dataset;

        // The no-data placeholder spans every column and belongs to none.
        if (columnId === undefined)
            return { kind: 'placeholder' };

        if (cell.tagName === 'TH')
            return { kind: 'header', columnId };

        const row = cell.parentElement;

        if (!(row instanceof HTMLTableRowElement))
            return undefined;

        const rowIndex = this.#getRowIndex(row);

        if (rowIndex === undefined)
            return undefined;

        return { kind: 'body', rowIndex, columnId };
    }

    #findEventCell(event: Event): HTMLTableCellElement | undefined {
        if (!(event.target instanceof Element))
            return undefined;

        const cell = event.target.closest<HTMLTableCellElement>(
            'td[data-column-id], th[data-column-id], tr.no-data > td'
        );

        // A cell of this very table. A table a consumer rendered inside a cell owns its own cells, and the
        // closest match is the one that does.
        if (cell?.closest('table') !== this.#tableElement)
            return undefined;

        return cell;
    }

    #findPositionCell(): HTMLTableCellElement | undefined {
        const position = this.#position;

        if (position === undefined)
            return undefined;

        if (position.kind === 'placeholder')
            return this.#findPlaceholderCell();

        if (position.kind === 'header')
            return this.#findHeaderCell(position.columnId);

        return this.#findColumnCell(this.#findRow(position.rowIndex), position.columnId);
    }

    // The first cell in DOM order of the first data row, which is pin order — the leftmost cell the user
    // actually sees, and what the stop falls back to whenever the position no longer names anything.
    #findDefaultCell(): HTMLTableCellElement | undefined {
        const [firstRow] = this.#getRows();

        // The placeholder covers a table with no rows to navigate. A header covers the window before the
        // first items arrive, when there is neither a row nor a placeholder yet — landing there leaves
        // sorting and filtering usable while the rows load.
        return firstRow?.querySelector<HTMLTableCellElement>(':scope > td[data-column-id]') ??
            this.#findPlaceholderCell() ??
            this.#getHeaderCells()[0];
    }

    // Only reached for a header whose column has been hidden: the stop goes to the column on the right, or to
    // the one on the left when the hidden column was the last.
    #findHeaderNeighbor(): HTMLTableCellElement | undefined {
        if (this.#position?.kind !== 'header')
            return undefined;

        for (const columnId of this.#headerNeighborColumnIds) {
            const header = this.#findHeaderCell(columnId);

            if (header !== undefined)
                return header;
        }

        return undefined;
    }

    #findFirstBodyCell(columnId: string): HTMLTableCellElement | undefined {
        const [firstRow] = this.#getRows();

        return this.#findColumnCell(firstRow, columnId) ?? this.#findPlaceholderCell();
    }

    #findHeaderCell(columnId: string): HTMLTableCellElement | undefined {
        return this.#tableElement.querySelector<HTMLTableCellElement>(
            `:scope > thead > tr > th[data-column-id="${CSS.escape(columnId)}"]`
        ) ?? undefined;
    }

    #findColumnCell(row: HTMLTableRowElement | undefined, columnId: string): HTMLTableCellElement | undefined {
        return row?.querySelector<HTMLTableCellElement>(
            `:scope > td[data-column-id="${CSS.escape(columnId)}"]`
        ) ?? undefined;
    }

    #findPlaceholderCell(): HTMLTableCellElement | undefined {
        return this.#tableElement.querySelector<HTMLTableCellElement>(':scope > tbody > tr.no-data > td') ?? undefined;
    }

    #getHeaderCells(): HTMLTableCellElement[] {
        return [...this.#tableElement.querySelectorAll<HTMLTableCellElement>(
            ':scope > thead > tr > th[data-column-id]'
        )];
    }

    // Puts the focused row back inside the render window by scrolling to where a row of its index has to sit.
    // Only an estimate — it merely has to land close enough for virtualization to render the row — and the
    // stop is moved back, and the scroll corrected, once the row actually appears.
    #scrollToPositionRow(): void {
        if (this.#position?.kind !== 'body')
            return;

        this.#awaitingRow = true;
        this.#container.scrollTop = this.#position.rowIndex * this.#getRowHeight();
    }

    #scrollCellIntoView(cell: HTMLTableCellElement): void {
        const visibleBox = this.#getVisibleBox();
        const cellRect = cell.getBoundingClientRect();

        // The header sticks to the top of the container, so it covers the strip a plain "is it inside the
        // box" test would happily leave the focused cell under.
        const topLimit = visibleBox.top + this.#getHeaderHeight();

        // A header cell is held against that same top edge, so it is visible wherever the body has been
        // scrolled to and measuring it against the strip it forms would scroll the body for nothing.
        if (cell.tagName !== 'TH') {
            if (cellRect.top < topLimit)
                this.#container.scrollTop -= topLimit - cellRect.top;
            else if (cellRect.bottom > visibleBox.bottom)
                this.#container.scrollTop += cellRect.bottom - visibleBox.bottom;
        }

        // A pinned cell is held against an edge of the container and is visible wherever the body has been
        // scrolled to, so there is nothing to bring into view — and scrolling to it would move the body.
        if (cell.classList.contains('pinned-column'))
            return;

        // The pinned columns cover the left and right strips of the container the same way the header covers
        // the top one, so an unpinned cell has to clear them rather than the container's own edges.
        const leftLimit = visibleBox.left + this.#getPinnedWidth('left');
        const rightLimit = visibleBox.right - this.#getPinnedWidth('right');

        if (cellRect.left < leftLimit)
            this.#container.scrollLeft -= leftLimit - cellRect.left;
        else if (cellRect.right > rightLimit)
            this.#container.scrollLeft += cellRect.right - rightLimit;
    }

    // The box the container paints its content in. `getBoundingClientRect` measures the border box, which
    // counts in the scrollbars: the horizontal bar the columns bring out, and the vertical gutter this
    // container reserves whether or not it has a bar to put in it.
    #getVisibleBox(): { left: number; top: number; right: number; bottom: number } {
        const containerRect = this.#container.getBoundingClientRect();
        const left = containerRect.left + this.#container.clientLeft;
        const top = containerRect.top + this.#container.clientTop;

        return {
            left,
            top,
            right: left + this.#container.clientWidth,
            bottom: top + this.#container.clientHeight
        };
    }

    #getPinnedWidth(side: 'left' | 'right'): number {
        let width = 0;

        for (const th of this.#tableElement.querySelectorAll<HTMLTableCellElement>(':scope thead th')) {
            if (th.style[side] !== '')
                width += th.offsetWidth;
        }

        return width;
    }

    // Measured rather than taken from the row height the styles declare, so the estimate stays right if that
    // height ever changes — and because the row a focused cell sits in is the one being measured against.
    #getHeaderHeight(): number {
        const header = this.#tableElement.querySelector<HTMLElement>(':scope > thead');

        return header?.getBoundingClientRect().height ?? 0;
    }

    #getRowHeight(): number {
        const [firstRow] = this.#getRows();

        return firstRow?.offsetHeight ?? fallbackRowHeightPixels;
    }

    #findRow(rowIndex: number): HTMLTableRowElement | undefined {
        return this.#getRows().find(row => this.#getRowIndex(row) === rowIndex);
    }

    // The body rows that carry a keyboard contract. The no-data placeholder and the spacers virtualization
    // renders around its window carry none, and are skipped by exactly that.
    #getRows(): HTMLTableRowElement[] {
        const body = this.#tableElement.querySelector<HTMLTableSectionElement>(':scope > tbody');

        if (body === null)
            return [];

        return [...body.rows].filter(row => this.#getRowIndex(row) !== undefined);
    }

    #getRowIndex(row: HTMLTableRowElement): number | undefined {
        const rowIndex = Number(row.cells[0]?.dataset.rowIndex);

        // Negative is .NET reporting a row it could not place in the loaded set, which leaves it unreachable
        // for this pass rather than sending the keyboard to a position that belongs to another row.
        if (!Number.isSafeInteger(rowIndex) || rowIndex < 0)
            return undefined;

        return rowIndex;
    }

    public dispose(): void {
        this.#mutationObserver.disconnect();
        this.#container.removeEventListener('keydown', this.#keyDownListener);
        this.#container.removeEventListener('keyup', this.#keyUpListener);
        this.#container.removeEventListener('focusin', this.#focusInListener);
        this.#container.removeEventListener('focusout', this.#focusOutListener);
        this.#leadingBoundary?.removeEventListener('focusin', this.#boundaryFocusInListener);
        this.#trailingBoundary?.removeEventListener('focusin', this.#boundaryFocusInListener);
    }

    // A reshape invalidates every absolute row index, the focused cell's included, so the stop goes back to
    // the default cell. A position in the header row survives it: the gestures that cause a reshape live
    // there, and Shift+Enter multi-sort would be unusable if each added level threw the position out of the
    // header row.
    public resetFocusedCell(): void {
        if (this.#position?.kind === 'header')
            return;

        this.#position = undefined;
        this.#awaitingRow = false;

        const cell = this.#findDefaultCell();

        if (cell === undefined)
            return;

        this.#adopt(cell);

        // The stop moves always; the focus only when the table already held it, so a reset the user is not
        // in is invisible until their next Tab back in.
        if (this.#hasFocus)
            cell.focus();
    }

    // Called from .NET after the render an activation caused, which is the first moment content the handler
    // created is there to take the focus.
    public enterFocusedCell(): void {
        const cell = this.#findPositionCell();

        // Only while the cell itself still holds the focus: anywhere else, the user has moved on.
        if (cell === undefined || document.activeElement !== cell)
            return;

        // A cell holding nothing focusable is simply never entered: the focus stays on it, which is the same
        // state a cell ends in once the last of its widgets has been walked past.
        focusFirstFocusable(cell);
    }
}

// Tab and Shift+Tab are the table's; every other modifier on the key belongs to the browser, and this one
// moves focus without canceling the press, so claiming those would drag focus along with someone else's
// gesture.
function isPlainTab(event: KeyboardEvent): boolean {
    return event.key === 'Tab' && !event.ctrlKey && !event.altKey && !event.metaKey;
}

function findBoundary(sibling: Element | undefined): HTMLElement | undefined {
    if (sibling instanceof HTMLElement && sibling.classList.contains('tab-boundary'))
        return sibling;

    return undefined;
}

// The columns either side of a header, in the order a hidden column falls back to them: the one to the right,
// then the one to the left for a hidden column that was the last.
function readNeighborColumnIds(cell: HTMLTableCellElement): readonly string[] {
    return [cell.nextElementSibling, cell.previousElementSibling]
        .map(neighbor => (neighbor instanceof HTMLTableCellElement ? neighbor.dataset.columnId : undefined))
        .filter(columnId => columnId !== undefined);
}
