export class FocusTrap {
    // Elements of every attached trap on the page. The set is static, so all instances share it and can
    // decide among each other which trap holds the focus.
    static readonly #attachedElements = new Set<HTMLElement>();

    // The z-index that decides which trap paints on top sits on the element itself or on the nearest
    // ancestor setting one. Equal values fall back to document order, in which the later element paints
    // on top.
    static #getStackOrder(element: Element): number {
        for (let current: Element | undefined = element; current; current = current.parentElement ?? undefined) {
            const zIndex = Number.parseInt(getComputedStyle(current).zIndex, 10);
            if (!Number.isNaN(zIndex))
                return zIndex;
        }

        return 0;
    }

    static #isStackedAbove(element: Element, other: Element): boolean {
        const stackOrder = FocusTrap.#getStackOrder(element);
        const otherStackOrder = FocusTrap.#getStackOrder(other);

        if (stackOrder !== otherStackOrder)
            return stackOrder > otherStackOrder;

        return other.compareDocumentPosition(element) === Node.DOCUMENT_POSITION_FOLLOWING;
    }

    static #isTopmost(element: HTMLElement): boolean {
        for (const other of FocusTrap.#attachedElements) {
            if (other !== element && FocusTrap.#isStackedAbove(other, element))
                return false;
        }

        return true;
    }

    // The element the trap currently holds the focus in, or undefined while no element is attached.
    #element: HTMLElement | undefined;

    readonly #onFocusIn = (event: FocusEvent) => {
        const element = this.#element;
        if (!element)
            return;

        const { target } = event;
        if (!(target instanceof Element) || element.contains(target))
            return;

        if (!document.hasFocus())
            return;

        if (!FocusTrap.#isTopmost(element))
            return;

        // An element that does not block the page but is opened on top of the trap, for example a tool
        // window, is still meant to be used, so focus may move into it.
        const exemptElement = target.closest('[data-focus-trap-exempt]');
        if (exemptElement && FocusTrap.#isStackedAbove(exemptElement, element))
            return;

        element.focus({ preventScroll: true });
    };

    public attach(element: HTMLElement) {
        this.detach();

        this.#element = element;
        FocusTrap.#attachedElements.add(element);

        document.addEventListener('focusin', this.#onFocusIn);
    }

    public detach() {
        if (!this.#element)
            return;

        document.removeEventListener('focusin', this.#onFocusIn);

        FocusTrap.#attachedElements.delete(this.#element);
        this.#element = undefined;
    }

    public dispose() {
        this.detach();
    }
}
