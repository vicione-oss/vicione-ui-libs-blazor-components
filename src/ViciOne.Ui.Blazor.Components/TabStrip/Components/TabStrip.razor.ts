import { ComputedBackgroundColor } from '../../Scripts/ComputedBackgroundColor.ts';
import '../../Scripts/HtmlElementMixins.ts';
import '../../Scripts/StringMixins.ts';

export class TabStrip {
    readonly #dotNetObject: DotNet.DotNetObject;
    readonly #scrollContainer: HTMLElement;
    readonly #tabsViewport: HTMLElement | undefined;
    readonly #resizeObserver: ResizeObserver;
    readonly #intersectionObserver: IntersectionObserver;
    readonly #computedBackgroundColor = new ComputedBackgroundColor();
    #pointerFocus = false;

    // Captured before focusin so the flag is set in time. A pointerdown on (or within) a tab will
    // focus it; remember this so the following focusin does not scroll and cancel the click.
    readonly #onPointerDown = (event: PointerEvent) => {
        const { target } = event;
        this.#pointerFocus = target instanceof HTMLElement && target.closest('.tab') !== null;
    };

    readonly #onFocusIn = (event: FocusEvent) => {
        const wasPointerFocus = this.#pointerFocus;
        this.#pointerFocus = false;

        const { target } = event;
        if (!(target instanceof HTMLElement))
            return;

        if (wasPointerFocus)
            return;

        const focusedTab = target.closest<HTMLElement>('.tab');
        if (focusedTab && this.#getTabs().includes(focusedTab))
            this.scrollToActiveTab(focusedTab);
    };

    readonly #onKeyDown = (event: KeyboardEvent) => {
        if (event.key !== 'ArrowRight' && event.key !== 'ArrowLeft')
            return;

        const tabs = this.#getTabs();
        if (tabs.length === 0)
            return;

        const { activeElement } = document;
        const focusedTab = activeElement instanceof HTMLElement ?
            activeElement.closest<HTMLElement>('.tab') :
            null;

        if (focusedTab && !tabs.includes(focusedTab))
            return;

        const currentIndex = focusedTab ? tabs.indexOf(focusedTab) : -1;

        const nextIndex = currentIndex === -1 ?
            (event.key === 'ArrowRight' ? 0 : tabs.length - 1) :
            currentIndex + (event.key === 'ArrowRight' ? 1 : -1);

        if (nextIndex < 0 || nextIndex >= tabs.length)
            return;

        event.preventDefault();
        tabs[nextIndex].focus({ preventScroll: true });
    };

    readonly #onScroll = () => {
        this.#notifyScrollState();
    };

    // On resize both the scroll state and the gradient overlay backgrounds can change, so the
    // overflow backgrounds are refreshed here instead of each owning its own ResizeObserver. Each
    // ::before/::after fade color is only recomputed while its pseudo-element is on-screen (see
    // #applyOverflowBackground), so off-screen sides are skipped and probe points stay in-viewport.
    readonly #onResize = () => {
        this.#applyOverflowBackgrounds();
        this.#notifyScrollState();
    };

    // Recompute the fade colors whenever the tab strip's visibility changes, so each ::before/::after
    // pseudo-element resolves its color as soon as it is on-screen. The graduated thresholds re-run
    // the computation as more of the pseudo-elements scroll into view, letting probe points that were
    // off-screen resolve once they become visible.
    readonly #onIntersectionChange = () => {
        this.#applyOverflowBackgrounds();
    };

    constructor(dotNetObject: DotNet.DotNetObject, scrollContainer: HTMLElement) {
        this.#dotNetObject = dotNetObject;
        this.#scrollContainer = scrollContainer;
        this.#tabsViewport = this.#scrollContainer.parentElement ?? undefined;

        this.#scrollContainer.addEventListener('scroll', this.#onScroll, { passive: true });
        this.#scrollContainer.addEventListener('pointerdown', this.#onPointerDown, { capture: true });
        this.#scrollContainer.addEventListener('focusin', this.#onFocusIn);
        this.#scrollContainer.addEventListener('keydown', this.#onKeyDown);

        this.#resizeObserver = new ResizeObserver(this.#onResize);
        this.#resizeObserver.observe(this.#scrollContainer);

        // ComputedBackgroundColor probes the page with document.elementsFromPoint, which only
        // returns results while the probe point is inside the viewport. The fade of each overflow
        // gradient is painted by the ::before (left) and ::after (right) pseudo-elements, so each
        // one's color is resolved only while that pseudo-element itself is on-screen (see
        // #applyOverflowBackground). This observer just triggers a recompute as the tab strip
        // scrolls through the viewport; the graduated thresholds re-run it as more of the pseudo-
        // elements become visible, so probe points that were off-screen resolve once on-screen.
        this.#intersectionObserver = new IntersectionObserver(
            this.#onIntersectionChange,
            { threshold: [0, 0.25, 0.5, 0.75, 1] }
        );
        if (this.#tabsViewport)
            this.#intersectionObserver.observe(this.#tabsViewport);

        this.#notifyScrollState();
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
            const nextTab =
                tabs.find(tab => tab.getBoundingClientRect().left >= visibleRight - tolerance) ??
                tabs.find(tab => tab.getBoundingClientRect().right > visibleRight + tolerance);
            if (!nextTab)
                return;

            this.#scrollContainer.scrollBy({ left: nextTab.getBoundingClientRect().right - visibleRight, behavior: 'smooth' });
        } else {
            // Switch to `tabs.toReversed()` once the TS compiler/lib target supports it reliably.
            // eslint-disable-next-line unicorn/no-array-reverse
            const reversedTabs = [...tabs].reverse();
            const previousTab =
                reversedTabs.find(tab => tab.getBoundingClientRect().right <= visibleLeft + tolerance) ??
                reversedTabs.find(tab => tab.getBoundingClientRect().left < visibleLeft - tolerance);
            if (!previousTab)
                return;

            this.#scrollContainer.scrollBy({ left: previousTab.getBoundingClientRect().left - visibleLeft, behavior: 'smooth' });
        }
    }

    #getTabs(): HTMLElement[] {
        return [...this.#scrollContainer.querySelectorAll<HTMLElement>(':scope > .tabs > .tab')];
    }

    #getOverflowSize(): number {
        const raw = getComputedStyle(this.#scrollContainer).getPropertyValue('--tab-strip-overflow-size');
        const parsed = Number.parseFloat(raw);
        return Number.isFinite(parsed) ? parsed : 0;
    }

    #applyOverflowBackgrounds() {
        this.#applyOverflowBackground('::before', '--tabs-viewport-before-fade-color');
        this.#applyOverflowBackground('::after', '--tabs-viewport-after-fade-color');
    }

    // Resolves and applies the fade color for a single overflow gradient pseudo-element. The color is
    // probed at the pseudo-element's own position; ComputedBackgroundColor clips that region to the
    // viewport, so an off-screen pseudo-element simply resolves no color and is left unchanged.
    #applyOverflowBackground(pseudoElement: '::before' | '::after', customProperty: string) {
        const tabsViewport = this.#tabsViewport;
        if (!tabsViewport)
            return;

        const pseudoElementBounds = tabsViewport.getPseudoElementBoundingClientRect(pseudoElement);
        const backgroundColor = this.#computedBackgroundColor.resolve(tabsViewport, pseudoElementBounds);
        if (backgroundColor !== undefined && backgroundColor !== '')
            tabsViewport.style.setProperty(customProperty, backgroundColor);
    }

    #notifyScrollState() {
        const { scrollLeft, scrollWidth, clientWidth } = this.#scrollContainer;

        // Tolerance covers the trailing spacer inside .tabs (see TabStrip.razor.scss) plus
        // sub-pixel rounding, so the right scroll button reliably disables when fully scrolled.
        const endTolerance = 5;

        const canScrollLeft = scrollLeft > 0;
        const canScrollRight = scrollLeft + clientWidth < scrollWidth - endTolerance;

        void this.#dotNetObject.invokeMethodAsync('ScrollStateChangedAsync', canScrollLeft, canScrollRight);
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
        this.#intersectionObserver.disconnect();
        this.#scrollContainer.removeEventListener('scroll', this.#onScroll);
        this.#scrollContainer.removeEventListener('pointerdown', this.#onPointerDown, true);
        this.#scrollContainer.removeEventListener('focusin', this.#onFocusIn);
        this.#scrollContainer.removeEventListener('keydown', this.#onKeyDown);
    }
}
