import { ColumnWidth } from '../Models/ColumnWidth.cs.ts';
import { type CloseObserver } from '../Scripts/CloseObserver.ts';
import { KeyboardNavigation } from '../Scripts/KeyboardNavigation.ts';
import { type ResolvedColumnWidth } from '../Scripts/ResolvedColumnWidth.ts';
import { type TableColumn } from '../Scripts/TableColumn.ts';
import '../../../Scripts/StringMixins.ts';

const commitQuietPeriodMilliseconds = 150;

class AdvancedTable {
    readonly #tableElement: HTMLElement;

    readonly #dotNetObject: DotNet.DotNetObject;

    readonly #container: HTMLElement | undefined;

    // The DI-registered IResizeObserver delivers its sizes into .NET, which would put a circuit round trip in
    // front of every callback — roughly sixty a second while a window edge is dragged.
    readonly #resizeObserver: ResizeObserver;

    // A height-only callback must not re-flow: together with the container's permanently reserved scrollbar
    // gutter, that is what keeps the observer loop-free.
    #observedContainerWidth = -1;

    // A render landing between a write and its commit paints over the widths in the DOM, so the commit reads
    // them here.
    readonly #flexWidths = new Map<string, number>();

    #commitTimeoutId = 0;

    #commitAfterQuietPeriod = false;

    // Closing panels reaches every table on the page, so attaching must not do it — and no panel of this table
    // can be open yet.
    #flowedOnce = false;

    // Teardown of the in-flight column resize, or undefined while no resize is running. Held here because the
    // gesture ends in more ways than a pointerup: a canceled pointer, a lost capture, or the table being
    // disposed mid-drag all have to restore the page-wide cursor and text selection the drag installed.
    #endColumnResize: (() => void) | undefined;

    // Undefined only for a table with no scroll container to hold the keyboard focus, which is the same
    // condition that leaves the columns unflowed.
    readonly #keyboardNavigation: KeyboardNavigation | undefined;

    readonly #onContainerResize = (entries: ResizeObserverEntry[]): void => {
        const containerWidth = entries.at(-1)?.contentRect.width;

        if (containerWidth === undefined || containerWidth === this.#observedContainerWidth)
            return;

        this.#observedContainerWidth = containerWidth;

        this.#reflow();
    };

    readonly #onQuietPeriodElapsed = (): void => {
        this.#commitTimeoutId = 0;

        if (!this.#commitAfterQuietPeriod)
            return;

        this.#commitAfterQuietPeriod = false;
        this.#commitWidths();
    };

    readonly #onPointerDown = (event: PointerEvent): void => {
        const { target } = event;
        if (!(target instanceof HTMLElement))
            return;

        // Pressing the filter icon must not start the column drag-to-reorder (the <th> is draggable).
        // Suppressing the native drag here mirrors the resize-handle suppression below; the open panel
        // itself is guarded by toggling the owning <th> draggable=false in C# while it is open.
        if (target.closest('.column-filter-button')) {
            event.preventDefault();
            return;
        }

        if (!target.classList.contains('resize-handle'))
            return;

        // Prevent the column drag-to-reorder from activating
        event.preventDefault();

        // A second press can land before the running resize has ended — another finger, or a pen beside the
        // mouse. Replacing its teardown unrun would leak its listeners, and this gesture would save the
        // col-resize cursor as the one to restore.
        this.#endColumnResize?.();

        const th = target.closest('th');
        if (!th)
            return;

        const { columnId } = th.dataset;
        if (columnId === undefined || columnId === '')
            return;

        const allColumns = this.#getColumns();
        const draggedColumn = allColumns.find(column => column.columnId === columnId);
        if (!draggedColumn)
            return;

        const { col } = draggedColumn;

        // While any flex column is still unresolved the table is width: 100% and the flex <col>s carry
        // no width, so under `table-layout: fixed` the browser would stretch them to fill whatever the
        // drag frees up. Pin every column and the table itself to their current measured px as element
        // styles only — no interop, no re-render mid-gesture — so only the dragged column can move.
        const isDragStartedUnpinned = allColumns.some(column => column.col.style.width === '');
        if (isDragStartedUnpinned) {
            // Measured up front: interleaving the writes would force a synchronous layout of the whole table
            // on every following offsetWidth.
            const measuredColumns = allColumns.map(column => ({ column, width: column.th.offsetWidth }));

            for (const { column, width } of measuredColumns)
                column.col.style.width = `${width}px`;

            const measuredTotal = measuredColumns.reduce((total, measured) => total + measured.width, 0);

            this.#tableElement.style.width = `${measuredTotal}px`;
        }

        const startX = event.clientX;
        const startWidth = th.offsetWidth;
        const startTableWidth = this.#tableElement.offsetWidth;
        const minWidth = getMinimumWidth(col);

        const { pointerId } = event;

        target.setPointerCapture(pointerId);

        const savedCursor = document.body.style.cursor;
        const savedUserSelect = document.body.style.userSelect;
        document.body.style.cursor = 'col-resize';
        document.body.style.userSelect = 'none';

        const onPointerMove = (moveEvent: PointerEvent): void => {
            // Anchored to the pointer's viewport X captured at pointerdown — never the handle's own
            // moving position — so a runaway feedback loop is impossible. Clamped low at the column
            // min, unbounded high (growth overflows into horizontal scroll).
            const delta = Math.max(moveEvent.clientX - startX, minWidth - startWidth);
            col.style.width = `${startWidth + delta}px`;
            // Track the table width in lockstep: left at its old specified width, `table-layout: fixed`
            // would redistribute a shrink across the other columns instead of ending the table earlier.
            this.#tableElement.style.width = `${startTableWidth + delta}px`;

            // The width just written moves the sticky edge of every pinned column after the dragged one, and
            // a drag performs no render, so nothing else refreshes those offsets while it runs.
            this.updatePinnedOffsets();
        };

        // Restores everything the gesture installed. Idempotent: it clears the field first, so a pointerup
        // followed by a lostpointercapture — or a dispose racing either — runs the body once.
        const endResize = (): void => {
            if (this.#endColumnResize !== endResize)
                return;

            this.#endColumnResize = undefined;

            target.removeEventListener('pointermove', onPointerMove);
            target.removeEventListener('pointerup', onPointerUp);
            target.removeEventListener('pointercancel', onPointerCancel);
            target.removeEventListener('lostpointercapture', onPointerCancel);

            if (target.hasPointerCapture(pointerId))
                target.releasePointerCapture(pointerId);

            document.body.style.cursor = savedCursor;
            document.body.style.userSelect = savedUserSelect;
        };

        // A canceled or lost pointer ends the gesture without a width to commit: the browser has stopped
        // sending moves, so the last width written is not one the user chose to finish on.
        const onPointerCancel = (): void => {
            endResize();
        };

        const onPointerUp = (): void => {
            endResize();

            // A commit queued before the gesture would report widths the drag has already replaced.
            this.#cancelPendingCommit();

            const committedColumns = isDragStartedUnpinned ?
                allColumns.filter(column => column.col.dataset.minWidth !== undefined) :
                [draggedColumn];

            this.#commitColumnWidths(
                committedColumns.map(column => new ColumnWidth(column.columnId, column.th.offsetWidth)),
                columnId
            );
        };

        this.#endColumnResize = endResize;

        target.addEventListener('pointermove', onPointerMove);
        target.addEventListener('pointerup', onPointerUp);
        target.addEventListener('pointercancel', onPointerCancel);
        target.addEventListener('lostpointercapture', onPointerCancel);
    };

    constructor(tableElement: HTMLElement, dotNetObject: DotNet.DotNetObject) {
        this.#tableElement = tableElement;
        this.#dotNetObject = dotNetObject;
        this.#container = tableElement.closest<HTMLElement>('.inner-table-container') ?? undefined;
        this.#tableElement.addEventListener('pointerdown', this.#onPointerDown);

        this.#resizeObserver = new ResizeObserver(this.#onContainerResize);

        if (this.#container) {
            this.#resizeObserver.observe(this.#container);
            this.#keyboardNavigation = new KeyboardNavigation(this.#container, tableElement, dotNetObject);
        }
    }

    #reflow(): void {
        const containerWidth = this.#getContainerWidth();

        if (containerWidth <= 0)
            return;

        const columns = this.#getColumns();
        const flexColumns = columns.filter(column => column.col.dataset.flex !== undefined);

        if (flexColumns.length === 0)
            return;

        const fixedTotal = columns
            .filter(column => column.col.dataset.flex === undefined)
            .reduce((total, column) => total + (column.col.style.width.toPixels() ?? 0), 0);

        const resolvedColumns = resolveFlexWidths(containerWidth - fixedTotal, flexColumns);

        this.#flexWidths.clear();

        let total = fixedTotal;

        for (const { flexColumn, width } of resolvedColumns) {
            flexColumn.col.style.width = toCssPixels(width);
            this.#flexWidths.set(flexColumn.columnId, width);
            total += width;
        }

        // Left at its previous value, `table-layout: fixed` would stretch or squeeze the columns straight back
        // to it.
        this.#tableElement.style.width = toCssPixels(total);

        // These writes skip a Blazor render, so nothing else refreshes the sticky offsets.
        this.updatePinnedOffsets();

        if (this.#flowedOnce) {
            // A re-flow moves the header a filter panel is anchored to, and a container change — a dialog, a
            // sidebar, a fullscreen toggle — reaches no window-resize listener.
            closeObservers.requestCloseAll();
        }

        this.#flowedOnce = true;

        this.#scheduleCommit();
    }

    // The content box the columns have to fill, in the same sub-pixel terms the browser laid the table out in.
    // clientWidth is a whole number, so it reports a container that is up to half a pixel wider or narrower
    // than the one the columns actually sit in — and the columns would be written to that wrong width. The
    // observer's contentRect carries the fractional value; clientWidth is left as the fallback for a re-flow
    // that runs before the first callback has delivered one.
    #getContainerWidth(): number {
        if (this.#observedContainerWidth >= 0)
            return this.#observedContainerWidth;

        return this.#container?.clientWidth ?? 0;
    }

    // Leading edge first, so .NET holds usable widths throughout a continuous resize; the value after the quiet
    // period is the one that has to survive the next render.
    #scheduleCommit(): void {
        if (this.#commitTimeoutId === 0) {
            this.#commitWidths();
        } else {
            clearTimeout(this.#commitTimeoutId);
            this.#commitAfterQuietPeriod = true;
        }

        this.#commitTimeoutId = setTimeout(this.#onQuietPeriodElapsed, commitQuietPeriodMilliseconds);
    }

    #cancelPendingCommit(): void {
        if (this.#commitTimeoutId !== 0)
            clearTimeout(this.#commitTimeoutId);

        this.#commitTimeoutId = 0;
        this.#commitAfterQuietPeriod = false;
    }

    #commitWidths(): void {
        if (this.#flexWidths.size === 0)
            return;

        const columnWidths: ColumnWidth[] = [];

        for (const [columnId, width] of this.#flexWidths)
            columnWidths.push(new ColumnWidth(columnId, width));

        this.#commitColumnWidths(columnWidths, null);
    }

    // eslint-disable-next-line @typescript-eslint/no-restricted-types -- the value crosses to .NET, where the absence of a user-fixed column is null
    #commitColumnWidths(columnWidths: ColumnWidth[], userFixedColumnId: string | null): void {
        void this.#dotNetObject.invokeMethodAsync('SetColumnWidthsAsync', columnWidths, userFixedColumnId)
            .catch(() => {/* circuit gone, nothing left to commit to */});
    }

    #getColumns(): TableColumn[] {
        const columns: TableColumn[] = [];

        for (const th of this.#tableElement.querySelectorAll<HTMLTableCellElement>(':scope thead th')) {
            const { columnId } = th.dataset;
            if (columnId === undefined)
                continue;

            const col = this.#tableElement.querySelector<HTMLTableColElement>(
                `:scope colgroup col[data-column-id="${CSS.escape(columnId)}"]`
            );

            if (col)
                columns.push({ columnId, th, col });
        }

        return columns;
    }

    public dispose(): void {
        // A table disposed mid-drag would otherwise leave the page-wide cursor and text selection behind.
        this.#endColumnResize?.();

        this.#resizeObserver.disconnect();
        this.#cancelPendingCommit();
        this.#tableElement.removeEventListener('pointerdown', this.#onPointerDown);
        this.#keyboardNavigation?.dispose();
    }

    /**
     Sends the keyboard back to the default cell. Called from .NET after a reshape: a remembered position
     would then point at whichever item ended up in that row, and a gesture would act on that one.
     */
    public resetFocusedCell(): void {
        this.#keyboardNavigation?.resetFocusedCell();
    }

    /**
     Moves the focus into the focused cell's content. Called from .NET after the render an activation caused,
     which is the first moment content the handler created is there to take it.
     */
    public enterFocusedCell(): void {
        this.#keyboardNavigation?.enterFocusedCell();
    }

    /**
     Called from .NET when the set of columns changed. There is no payload: the colgroup it has just rendered
     carries everything this needs.
     */
    public columnsChanged(): void {
        this.#reflow();
    }

    /**
     Fills the `--pin-left-{index}`/`--pin-right-{index}` custom properties the pinned headers and cells
     reference, so left pins stack from the left edge and right pins from the right edge without
     overlapping. The index is the header's position in `thead`, the same position the .NET side names the
     property by.
     */
    public updatePinnedOffsets(): void {
        // Each header carries its own measured width and the position .NET names its custom property by.
        // Every width is measured before the first write: interleaving the two would force a synchronous
        // reflow of the whole table on every following offsetWidth.
        const measuredHeaders = [...this.#tableElement.querySelectorAll<HTMLTableCellElement>(':scope thead th')]
            .map((th, index) => ({ th, index, width: th.offsetWidth }));

        let leftOffset = 0;
        for (const { th, index, width } of measuredHeaders) {
            if (th.style.left === '')
                continue;

            this.#tableElement.style.setProperty(`--pin-left-${index}`, `${leftOffset}px`);

            leftOffset += width;
        }

        // Right pins stack from the right edge, so this walks back to front. Reversing a copy would need
        // `toReversed()`, which the TypeScript compiler this project builds with does not resolve.
        let rightOffset = 0;
        for (let position = measuredHeaders.length - 1; position >= 0; position--) {
            const { th, index, width } = measuredHeaders[position];

            if (th.style.right === '')
                continue;

            this.#tableElement.style.setProperty(`--pin-right-${index}`, `${rightOffset}px`);

            rightOffset += width;
        }
    }
}

// The width a column may not be squeezed below. .NET writes the attribute as a bare number rather than as a
// CSS length, so `toPixels()` cannot read it. A column carrying no attribute is one that cannot be resized,
// and zero leaves it unconstrained where a minimum is asked for.
function getMinimumWidth(col: HTMLTableColElement): number {
    const parsed = Number.parseFloat(col.dataset.minWidth ?? '');

    return Number.isFinite(parsed) ? parsed : 0;
}

function resolveFlexWidths(leftover: number, flexColumns: readonly TableColumn[]): readonly ResolvedColumnWidth[] {
    const resolvedColumns: ResolvedColumnWidth[] = flexColumns.map(flexColumn => ({
        flexColumn,
        minimumWidth: getMinimumWidth(flexColumn.col),
        width: 0
    }));

    let available = Math.max(leftover, 0);
    let unclamped = resolvedColumns;

    while (unclamped.length > 0) {
        const share = available / unclamped.length;
        const clamped = unclamped.filter(resolvedColumn => resolvedColumn.minimumWidth > share);

        if (clamped.length === 0)
            break;

        for (const resolvedColumn of clamped) {
            resolvedColumn.width = resolvedColumn.minimumWidth;
            available -= resolvedColumn.minimumWidth;
        }

        unclamped = unclamped.filter(resolvedColumn => !clamped.includes(resolvedColumn));
    }

    if (unclamped.length === 0)
        return resolvedColumns;

    // Shared out at full precision rather than in whole pixels. A whole-pixel share is not the split the
    // browser itself had already made before this ran — `table-layout: fixed` divides the table exactly — so
    // rounding here moved every column edge by a fraction of a pixel the moment the first re-flow landed, which
    // is a visible jump on a table that has just been navigated to.
    const baseShare = available / unclamped.length;

    for (const [position, resolvedColumn] of unclamped.entries()) {
        const isLast = position === unclamped.length - 1;

        // The last one is given what is left rather than its own share, so floating-point drift cannot leave
        // the columns adding up to slightly less than the space they were handed.
        resolvedColumn.width = isLast ? available - (baseShare * (unclamped.length - 1)) : baseShare;
    }

    return resolvedColumns;
}

// A width as a CSS pixel length, carrying the same three decimals .NET writes its own widths with, so a width
// this computes and the same width rendered back by a later Blazor pass agree down to the last digit.
function toCssPixels(width: number): string {
    return `${Number(width.toFixed(3))}px`;
}

export function attach(tableElement: HTMLElement, dotNetObject: DotNet.DotNetObject): AdvancedTable {
    return new AdvancedTable(tableElement, dotNetObject);
}

// Distance the panel keeps from the edge of its containing block when it has to be clamped back inside.
const containingBlockMargin = 8;

// Registry of the live close observers, keyed by the id handed back to .NET. Owning the teardown here keeps
// .NET holding nothing but an integer, so a circuit that dies without disposing leaks no listener reference.
class CloseObserverRegistry {
    readonly #observersById = new Map<number, CloseObserver>();

    #nextId = 0;

    add(cleanup: (id: number) => void, requestClose: () => void): number {
        const id = this.#nextId++;

        this.#observersById.set(id, {
            cleanup() {
                cleanup(id);
            },
            requestClose
        });

        return id;
    }

    remove(id: number): void {
        this.#observersById.delete(id);
    }

    stop(id: number): void {
        this.#observersById.get(id)?.cleanup();
    }

    // Snapshotted first, because closing an observer removes the entry being iterated.
    requestCloseAll(): void {
        // eslint-disable-next-line unicorn/no-useless-spread -- the snapshot is what lets a close handler remove entries while this iterates
        for (const observer of [...this.#observersById.values()])
            observer.requestClose();
    }
}

const closeObservers = new CloseObserverRegistry();

// A fixed-position element resolves its left/top against the viewport only while no ancestor establishes a
// containing block for it. Every one of these properties does establish one, and a dialog sets backdrop-filter
// to frost its own background — so a panel opened inside a dialog resolves against the dialog's box, and
// viewport coordinates would displace it by exactly the dialog's own offset.
const containingBlockProperties = ['transform', 'filter', 'backdropFilter', 'perspective'] as const;

function hasContainingBlockStyle(style: CSSStyleDeclaration): boolean {
    if (containingBlockProperties.some(property => style[property] !== 'none'))
        return true;

    // Will-change names the property the element is about to animate, and contain in its paint or layout forms
    // has the same effect as applying one of the properties above outright.
    return containingBlockProperties.some(property => style.willChange.includes(property)) ||
        style.contain.includes('paint') ||
        style.contain.includes('layout') ||
        style.contain === 'strict' ||
        style.contain === 'content';
}

// The box a fixed-position descendant of `element` is positioned and clamped against. Width and height come
// from documentElement rather than window when the viewport is that box: the window values include the
// scrollbars, so a page with a vertical scrollbar would let the panel overhang far enough right for its border
// to sit under the scrollbar and disappear.
function getFixedContainingBlockRect(element: HTMLElement): DOMRectReadOnly {
    for (let ancestor = element.parentElement; ancestor !== null; ancestor = ancestor.parentElement) {
        if (hasContainingBlockStyle(getComputedStyle(ancestor)))
            return ancestor.getBoundingClientRect();

    }

    return new DOMRectReadOnly(0, 0, document.documentElement.clientWidth, document.documentElement.clientHeight);
}

export function getFilterPanelPosition(button: HTMLElement): { left: number; bottom: number } {
    const boundingClientRect = button.getBoundingClientRect();
    const containingBlockRect = getFixedContainingBlockRect(button);

    return {
        left: boundingClientRect.left - containingBlockRect.left,
        bottom: boundingClientRect.bottom - containingBlockRect.top
    };
}

export function clampPanelLeft(panel: HTMLElement, preferredLeft: number): number {
    const maximumLeft = getFixedContainingBlockRect(panel).width - panel.offsetWidth - containingBlockMargin;

    return Math.max(0, Math.min(preferredLeft, maximumLeft));
}

export function clampPanelTop(panel: HTMLElement, preferredTop: number): number {
    const maximumTop = getFixedContainingBlockRect(panel).height - panel.offsetHeight - containingBlockMargin;

    return Math.max(0, Math.min(preferredTop, maximumTop));
}

export function startObserveClose(
    panel: HTMLElement,
    trigger: HTMLElement,
    dotNetObject: DotNet.DotNetObject
): number {
    let observerId = -1;

    const cleanup = (id: number): void => {
        globalThis.removeEventListener('pointerdown', pointerDownListener);
        globalThis.removeEventListener('scroll', scrollListener, true);
        globalThis.removeEventListener('resize', resizeListener);
        closeObservers.remove(id);
    };

    // The panel closes on the .NET side, so the listeners fire and forget. A rejection means the circuit is
    // already gone, which is exactly the case where there is nothing left to close.
    const requestClose = (): void => {
        cleanup(observerId);
        void dotNetObject.invokeMethodAsync('ClosePanelAsync').catch(() => {/* circuit gone */});
    };

    const pointerDownListener = (event: PointerEvent): void => {
        const target = event.target as Node;
        if (!panel.contains(target) && !trigger.contains(target))
            requestClose();
    };

    const scrollListener = (event: Event): void => {
        // Ignore a scroll originating inside the panel (e.g. a scrollable filter body) — only an outside
        // scroll dismisses. The listener is capture-phase, so the scrolled element is the event target.
        const target = event.target as Node;
        if (panel.contains(target))
            return;

        requestClose();
    };

    const resizeListener = (): void => {
        requestClose();
    };

    observerId = closeObservers.add(cleanup, requestClose);

    globalThis.addEventListener('pointerdown', pointerDownListener);
    globalThis.addEventListener('scroll', scrollListener, { capture: true, passive: true });
    globalThis.addEventListener('resize', resizeListener, { passive: true });

    return observerId;
}

export function stopObserveClose(id: number): void {
    closeObservers.stop(id);
}

// Additional exports to allow child components to call JavaScript code without the need
// to implement their own JavaScript module with all the overhead that comes with it.
export { focusFirstFocusable } from '../Scripts/Focusable.ts';

