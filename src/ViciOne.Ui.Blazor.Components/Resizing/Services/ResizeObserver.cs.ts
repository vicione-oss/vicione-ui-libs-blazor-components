/**
 * Allows to subscribe to elements by their property 'data-observer-id'.
 * When a resize occurs, the subcriber is notified and receives the new dimension.
 */
export class HtmlElementResizeObserver {
    readonly #observers = new Map<string, DotNet.DotNetObject>();

    readonly #resizeObserver = new ResizeObserver(entries => {
        for (const entry of entries) {
            const observerId = entry.target.getAttribute('data-observer-id');
            if (observerId) {
                const dotNetRef = this.#observers.get(observerId);

                void dotNetRef?.invokeMethodAsync(
                    'SizeChanged',
                    observerId,
                    entry.contentRect
                );
            }
        }
    });

    /**
    * Adds an element to the list of observer elements.
    * Beware: Each HTML Element can only have a single watcher at a time as of now.
    * If more are needed, code needs to be refactored.
    * @param {HTMLElement} elementRef The reference to the element which should be observed
    * @param {DotNetRef} dotNetObjectReference The reference to the caller which should be notified
    * on a resize of the element
    */
    public observe(elementRef: HTMLElement | undefined, dotNetObjectReference: DotNet.DotNetObject): void {
        if (!elementRef?.dataset.observerId || !dotNetObjectReference)
            return;

        this.#observers.set(elementRef.dataset.observerId, dotNetObjectReference);
        this.#resizeObserver.observe(elementRef);
    }

    /**
     * Removes an element from the list of observed elements via it's reference.
     * @param elementRef The reference to the element which should no be observed
     */
    public unobserve(elementRef: HTMLElement | undefined): void {
        if (!elementRef)
            return;

        this.#resizeObserver.unobserve(elementRef);

        const id = elementRef.dataset.observerId!;
        this.#observers.delete(id);
    }
}

/**
 * Creates and returns a new instance
 * @returns The created observer
 */
export function createInstance(): HtmlElementResizeObserver {
    return new HtmlElementResizeObserver();
}
