class TabStrip {
    readonly #dotNetObject: DotNet.DotNetObject;
    readonly #scrollContainer: HTMLElement;
    readonly #resizeObserver: ResizeObserver;

    constructor(dotNetObject: DotNet.DotNetObject, scrollContainer: HTMLElement) {
        this.#dotNetObject = dotNetObject;
        this.#scrollContainer = scrollContainer;

        this.#scrollContainer.addEventListener('scroll', this.#onScrollOrResize, { passive: true });

        this.#resizeObserver = new ResizeObserver(this.#onScrollOrResize);
        this.#resizeObserver.observe(this.#scrollContainer);

        this.#notifyScrollState();
    }

    public scrollToActiveTab(tabElement: HTMLElement | undefined, behavior: ScrollBehavior = 'smooth') {
        if (!tabElement)
            return;

        const overflowSize = this.#getOverflowSize();
        const { scrollLeft, scrollWidth, clientWidth } = this.#scrollContainer;
        const maxScrollLeft = Math.max(0, scrollWidth - clientWidth);

        // The visible area excludes whichever overflow gradient is currently shown.
        const hasLeftGradient = scrollLeft > 0;
        const hasRightGradient = scrollLeft < maxScrollLeft - 1;

        const containerRect = this.#scrollContainer.getBoundingClientRect();
        const tabRect = tabElement.getBoundingClientRect();
        const visibleLeft = containerRect.left + (hasLeftGradient ? overflowSize : 0);
        const visibleRight = containerRect.right - (hasRightGradient ? overflowSize : 0);

        // Sub-pixel tolerance: if the tab is already fully visible, do not scroll at all.
        const tolerance = 1;
        if (tabRect.left >= visibleLeft - tolerance && tabRect.right <= visibleRight + tolerance)
            return;

        // Scroll the minimum distance to make the tab visible: if it is clipped on the left,
        // align its right edge just inside the right gradient; otherwise (clipped on the right
        // or wider than the viewport) align its left edge just inside the left gradient. The
        // browser clamps the resulting scrollLeft, so the first/last tabs stay flush with the
        // start/end of the scroll range.
        const isClippedOnLeft = tabRect.left < visibleLeft - tolerance;
        const isClippedOnRight = tabRect.right > visibleRight + tolerance;
        const left = isClippedOnLeft && !isClippedOnRight ?
            tabElement.offsetLeft + tabElement.offsetWidth - clientWidth + overflowSize :
            tabElement.offsetLeft - overflowSize;

        this.#scrollContainer.scrollTo({ left, behavior });
    }

    public scrollLeft() {
        this.#scrollToAdjacentTab('left');
    }

    public scrollRight() {
        this.#scrollToAdjacentTab('right');
    }

    public dispose() {
        this.#resizeObserver.disconnect();
        this.#scrollContainer.removeEventListener('scroll', this.#onScrollOrResize);
    }

    #scrollToAdjacentTab(direction: 'left' | 'right') {
        const tabs = this.#getTabs();
        if (tabs.length === 0)
            return;

        const containerRect = this.#scrollContainer.getBoundingClientRect();
        const overflowSize = this.#getOverflowSize();

        // Treat the area covered by the gradient as not-visible, so the adjacent tab is
        // scrolled clear of the gradient instead of just to the container edge.
        const visibleLeft = containerRect.left + overflowSize;
        const visibleRight = containerRect.right - overflowSize;

        // Sub-pixel tolerance to avoid treating nearly-visible tabs as clipped.
        const tolerance = 1;

        if (direction === 'right') {
            const nextTab = tabs.find(tab => tab.getBoundingClientRect().right > visibleRight + tolerance);
            if (!nextTab)
                return;

            this.#scrollContainer.scrollBy({ left: nextTab.getBoundingClientRect().right - visibleRight, behavior: 'smooth' });
        } else {
            const previousTab = [...tabs].reverse().find(tab => tab.getBoundingClientRect().left < visibleLeft - tolerance);
            if (!previousTab)
                return;

            this.#scrollContainer.scrollBy({ left: previousTab.getBoundingClientRect().left - visibleLeft, behavior: 'smooth' });
        }
    }

    #getTabs(): HTMLElement[] {
        return Array.from(this.#scrollContainer.querySelectorAll<HTMLElement>(':scope > .tabs > .tab'));
    }

    #getOverflowSize(): number {
        const raw = getComputedStyle(this.#scrollContainer).getPropertyValue('--tab-strip-overflow-size');
        const parsed = parseFloat(raw);
        return Number.isFinite(parsed) ? parsed : 0;
    }

    readonly #onScrollOrResize = () => {
        this.#notifyScrollState();
    };

    #notifyScrollState() {
        const { scrollLeft, scrollWidth, clientWidth } = this.#scrollContainer;

        // Tolerance covers the trailing spacer inside .tabs (see TabStrip.razor.scss) plus
        // sub-pixel rounding, so the right scroll button reliably disables when fully scrolled.
        const endTolerance = 5;

        const canScrollLeft = scrollLeft > 0;
        const canScrollRight = scrollLeft + clientWidth < scrollWidth - endTolerance;

        void this.#dotNetObject.invokeMethodAsync('ScrollStateChangedAsync', canScrollLeft, canScrollRight);
    }
}

export function attach(dotNetObject: DotNet.DotNetObject, scrollContainer: HTMLElement) {
    return new TabStrip(dotNetObject, scrollContainer);
}
