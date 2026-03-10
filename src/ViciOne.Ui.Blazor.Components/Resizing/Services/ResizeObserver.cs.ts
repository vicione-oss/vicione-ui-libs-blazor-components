/**
 * Allows to subscribe to elements by their property 'data-observer-id'.
 * When a resize occurs, the subcriber is notified and receives the new dimension.
 */
export class HtmlElementResizeObserver {
    readonly #observers = new Map<string, ObserverDetails>();

    readonly #resizeObserver = new ResizeObserver(
        (entries: ResizeObserverEntry[]) => {
            for (const entry of entries) {
                if (!(entry.target instanceof HTMLElement))
                    continue;

                const { observerId } = entry.target.dataset;

                if (!observerId)
                    continue;

                const observerDetails = this.#observers.get(observerId);

                let style;
                if (observerDetails?.includeStyle) {
                    const computedStyle = window.getComputedStyle(entry.target);

                    style = {
                        marginLeft: parseFloat(computedStyle.marginLeft) || 0,
                        marginRight: parseFloat(computedStyle.marginRight) || 0,
                        marginTop: parseFloat(computedStyle.marginTop) || 0,
                        marginBottom: parseFloat(computedStyle.marginBottom) || 0,
                        paddingLeft: parseFloat(computedStyle.paddingLeft) || 0,
                        paddingRight: parseFloat(computedStyle.paddingRight) || 0,
                        paddingTop: parseFloat(computedStyle.paddingTop) || 0,
                        paddingBottom: parseFloat(computedStyle.paddingBottom) || 0,
                        borderLeft: parseFloat(computedStyle.borderLeft) || 0,
                        borderRight: parseFloat(computedStyle.borderRight) || 0,
                        borderTop: parseFloat(computedStyle.borderTop) || 0,
                        borderBottom: parseFloat(computedStyle.borderBottom) || 0
                    };
                }

                void observerDetails?.dotNetObject.invokeMethodAsync(
                    'SizeChanged',
                    observerId,
                    entry.contentRect,
                    style
                );
            }
        }
    );

    /**
    * Adds an element to the list of observer elements.
    * Beware: Each HTML Element can only have a single watcher at a time as of now.
    * If more are needed, code needs to be refactored.
    * @param {HTMLElement} elementReference The reference to the element which should be observed
    * @param {string} id The id which will be used to report back changes to the element
    * @param {DotNetRef} dotNetObject The reference to the caller which should be notified on a resize of the element
    */
    public observe = (
        elementReference: HTMLElement | undefined,
        id: string | undefined,
        dotNetObject: DotNet.DotNetObject,
        includeStyle: boolean
    ): void => {
        if (!elementReference?.dataset || !id || !dotNetObject)
            return;

        elementReference.dataset.observerId = id;
        this.#observers.set(id, new ObserverDetails(dotNetObject, includeStyle));

        this.#resizeObserver.observe(elementReference);
    };

    /**
     * Removes an element from the list of observed elements via it's reference.
     * @param elementRef The reference to the element which should no be observed
     */
    public unobserve(elementRef: HTMLElement | undefined): void {
        if (!elementRef)
            return;

        this.#resizeObserver.unobserve(elementRef);

        const id = elementRef.dataset.observerId ?? '';
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

class ObserverDetails {
    constructor(readonly dotNetObject: DotNet.DotNetObject, readonly includeStyle: boolean) {}
}
