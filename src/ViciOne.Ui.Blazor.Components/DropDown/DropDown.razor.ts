export class DropDown {
    static get #maxVisibleItemCount() {
        return 10;
    }

    static get #minVisibleItemCount() {
        return 3;
    }

    readonly #containerElement: HTMLElement;

    #inputElement: HTMLElement | undefined = undefined;
    #highlightedIndex = -1;

    readonly #keyDown = (event: KeyboardEvent) => {
        if (!this.#isVisible())
            return;

        const items = this.#getItemElements();
        const count = items.length;

        const isArrowDown = event.key === 'ArrowDown';

        if ((isArrowDown || event.key === 'ArrowUp') && count > 0) {
            let currentIndex = this.#highlightedIndex >= 0 ?
                this.#highlightedIndex :
                this.#getMostRecentlySelectedIndex();

            if (currentIndex >= count)
                currentIndex = -1;

            if (isArrowDown)
                this.#setHighlight((currentIndex + 1) % count);
            else
                this.#setHighlight((currentIndex <= 0 ? count : currentIndex) - 1);

            event.preventDefault();

        } else if (event.key === 'Enter') {
            if (this.#highlightedIndex >= 0 && this.#highlightedIndex < count) {
                event.preventDefault();
                event.stopPropagation();

                items[this.#highlightedIndex].click();
                this.#setHighlight(-1);
            }

        } else {
            this.#setHighlight(-1);
        }
    };

    constructor(containerElement: HTMLElement) {
        this.#containerElement = containerElement;
    }

    #getItemElements(): HTMLElement[] {
        return [...this.#containerElement.querySelectorAll<HTMLElement>('.drop-down-item')];
    }

    #getItemMetrics(items: HTMLElement[]): { itemOuterHeight: number; containerVerticalExtra: number } | undefined {
        const itemStyle = getComputedStyle(items[0]);

        const itemOuterHeight = items[0].getBoundingClientRect().height +
            Number.parseFloat(itemStyle.marginTop) +
            Number.parseFloat(itemStyle.marginBottom);

        if (itemOuterHeight <= 0)
            return undefined;

        const containerStyle = getComputedStyle(this.#containerElement);

        const containerVerticalExtra =
            Number.parseFloat(containerStyle.borderTopWidth) +
            Number.parseFloat(containerStyle.borderBottomWidth) +
            Number.parseFloat(containerStyle.paddingTop) +
            Number.parseFloat(containerStyle.paddingBottom);

        return { itemOuterHeight, containerVerticalExtra };
    }

    #setMaxHeight(metrics: { itemOuterHeight: number; containerVerticalExtra: number }, visibleItemCount: number) {
        this.#containerElement.style.maxHeight =
            `${(metrics.itemOuterHeight * visibleItemCount) + metrics.containerVerticalExtra}px`;
    }

    #getOverflowParent(element: HTMLElement): HTMLElement | undefined {
        let parent = element.parentElement;

        while (parent) {
            const style = getComputedStyle(parent);
            const { overflowY } = style;

            if (overflowY === 'auto' || overflowY === 'scroll')
                return parent;

            parent = parent.parentElement;
        }

        return undefined;
    }

    #isVisible(): boolean {
        return this.#containerElement.classList.contains('visible');
    }

    #getMostRecentlySelectedIndex(): number {
        return this.#getItemElements().findIndex(item => item.classList.contains('most-recently-selected'));
    }

    #setHighlight(index: number) {
        const items = this.#getItemElements();

        for (const item of items)
            item.classList.remove('highlighted');

        this.#highlightedIndex = index;

        if (this.#highlightedIndex >= 0 && this.#highlightedIndex < items.length) {
            items[this.#highlightedIndex].classList.add('highlighted');
            items[this.#highlightedIndex].scrollIntoView({ block: 'nearest' });
        }
    }

    public attachInputElement(inputElement: HTMLElement | undefined) {
        this.detachInputElement();

        if (inputElement === undefined)
            return;

        this.#inputElement = inputElement;
        this.#inputElement.addEventListener('keydown', this.#keyDown);
    }

    public detachInputElement() {
        if (!this.#inputElement)
            return;

        this.#inputElement.removeEventListener('keydown', this.#keyDown);
        this.#inputElement = undefined;
    }

    public setMinimumWidth() {
        const wrapper = this.#containerElement.parentElement;

        if (!wrapper)
            return;

        // Use of getComputedStyle instead of offsetWidth because offsetWidth is rounded to integer,
        // while getComputedStyle returns the exact value(including decimals) to prevent jumping behavior.
        //
        // Also, it is important, that at this point, that the CSS modifier ".visible" is NOT SET,
        // because it applies absolute positioning, which causes the container element to have the width of the wrapper instead of the items (right: 0).
        const computedWidth = getComputedStyle(this.#containerElement).width;

        // The following statement set minWidth because:
        // - the drop-down should not shrink, caused by filtered items, while typing.
        // - the drop-down should be allowed to get wider, when outer styling causes it (e.g. TagBox, that changes it's width, while typing).
        wrapper.style.minWidth = computedWidth;

        // Revert rule applied in scss to ensure, that items take full width, when no vertical scrollbar is present.
        this.#containerElement.style.scrollbarGutter = 'unset';
    }

    public updatePlacement() {
        const inputElement = this.#inputElement;

        if (!inputElement)
            return;

        const container = this.#containerElement;
        const wasPlaced = container.classList.contains('placed');

        // Reset any previous placement so the direction is recomputed from scratch
        // and the menu stays hidden (not 'placed') until we reveal it below.
        //
        // The max-height is also cleared so that, until this method computes the
        // real value below, the CSS rule for '.visible:not(.placed)' keeps the menu
        // collapsed. Otherwise a stale/oversized max-height would let the menu
        // briefly expand (and flash a scrollbar) the moment the 'visible' class is
        // applied, before the final height is calculated.
        container.classList.remove('placed');
        container.style.top = '';
        container.style.bottom = '';
        container.style.maxHeight = '';

        const items = this.#getItemElements();

        if (items.length === 0)
            return;

        const metrics = this.#getItemMetrics(items);

        if (!metrics)
            return;

        const inputRect = inputElement.getBoundingClientRect();

        // The control element is the whole control (e.g. tag-box/combo-box root).
        // In a multi-line control (like the tag-box) the input sits on the last
        // wrapped line, so the menu must be measured and anchored against the top
        // of the control rather than the input line to sit above all its content.
        const controlElement = container.parentElement?.parentElement;
        const controlRect = controlElement ? controlElement.getBoundingClientRect() : inputRect;

        const overflowParent = this.#getOverflowParent(inputElement);
        const overflowRect = overflowParent?.getBoundingClientRect();

        const viewportTop = overflowRect ? overflowRect.top : 0;
        const viewportBottom = overflowRect ? overflowRect.bottom : document.documentElement.clientHeight;

        const spaceBelow = viewportBottom - inputRect.bottom;
        const spaceAbove = controlRect.top - viewportTop;

        const maxVisibleCount = Math.min(items.length, DropDown.#maxVisibleItemCount);
        const minVisibleCount = Math.min(items.length, DropDown.#minVisibleItemCount);

        // How many items can be shown within the given vertical space, capped at
        // the maximum we ever display.
        const countItemsFittingIn = (availableSpace: number) => {
            const fittingItems = Math.floor((availableSpace - metrics.containerVerticalExtra) / metrics.itemOuterHeight);

            return Math.max(0, Math.min(fittingItems, maxVisibleCount));
        };

        const itemsFittingBelow = countItemsFittingIn(spaceBelow);
        const itemsFittingAbove = countItemsFittingIn(spaceAbove);

        let shouldDropUp = false;
        let visibleItemCount: number;

        // Prefer dropping down; only drop up when doing so shows more items and
        // dropping down cannot even fit the minimum we want to display.
        if (itemsFittingBelow >= minVisibleCount) {
            visibleItemCount = itemsFittingBelow;
        } else if (itemsFittingAbove > itemsFittingBelow) {
            shouldDropUp = true;
            visibleItemCount = itemsFittingAbove;
        } else {
            visibleItemCount = itemsFittingBelow;
        }

        visibleItemCount = Math.max(minVisibleCount, Math.min(visibleItemCount, maxVisibleCount));

        this.#setMaxHeight(metrics, visibleItemCount);

        const mostRecentlySelectedItem = items.find(item => item.classList.contains('most-recently-selected'));

        if (!wasPlaced && mostRecentlySelectedItem)
            container.scrollTop = mostRecentlySelectedItem.offsetTop;

        if (shouldDropUp) {
            const wrapper = container.parentElement;

            if (wrapper) {
                const wrapperRect = wrapper.getBoundingClientRect();
                const marginTop = Number.parseFloat(getComputedStyle(wrapper).marginTop);
                const gap = Number.isNaN(marginTop) ? 0 : marginTop;

                // Anchor the menu above the whole control, leaving the same gap as
                // the drop-down direction so the entire control stays visible.
                container.style.top = 'auto';
                container.style.bottom = `${wrapperRect.bottom - controlRect.top + gap}px`;
            }
        }

        container.classList.add('placed');
    }

    public resetPlacement() {
        this.#setHighlight(-1);

        this.#containerElement.classList.remove('placed');
        this.#containerElement.style.top = '';
        this.#containerElement.style.bottom = '';
        this.#containerElement.style.maxHeight = '';
    }

    public dispose() {
        this.detachInputElement();
    }
}
