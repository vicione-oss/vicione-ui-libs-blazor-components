/**
 Allows to subscribe to elements by their property 'data-observer-id'.
 When a resize occurs, the subcriber is notified and receives the new dimension.
 */
const parseCssLength = (value: string): number => {
    const parsed = Number.parseFloat(value);
    return Number.isNaN(parsed) ? 0 : parsed;
};

export class HtmlElementResizeObserver {
    readonly #observers = new Map<string, ObserverDetails>();

    readonly #resizeObserver = new ResizeObserver(
        (entries: ResizeObserverEntry[]) => {
            for (const entry of entries) {
                if (!(entry.target instanceof HTMLElement))
                    continue;

                const { observerId } = entry.target.dataset;

                if (observerId === undefined || observerId === '')
                    continue;

                const observerDetails = this.#observers.get(observerId);

                let style;
                if (observerDetails?.includeStyle) {
                    const computedStyle = globalThis.getComputedStyle(entry.target);

                    style = {
                        marginLeft: parseCssLength(computedStyle.marginLeft),
                        marginRight: parseCssLength(computedStyle.marginRight),
                        marginTop: parseCssLength(computedStyle.marginTop),
                        marginBottom: parseCssLength(computedStyle.marginBottom),
                        paddingLeft: parseCssLength(computedStyle.paddingLeft),
                        paddingRight: parseCssLength(computedStyle.paddingRight),
                        paddingTop: parseCssLength(computedStyle.paddingTop),
                        paddingBottom: parseCssLength(computedStyle.paddingBottom),
                        borderLeft: parseCssLength(computedStyle.borderLeft),
                        borderRight: parseCssLength(computedStyle.borderRight),
                        borderTop: parseCssLength(computedStyle.borderTop),
                        borderBottom: parseCssLength(computedStyle.borderBottom)
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
    Adds an element to the list of observer elements.
    Beware: Each HTML Element can only have a single watcher at a time as of now.
    If more are needed, code needs to be refactored.
    @param elementReference The reference to the element which should be observed
    @param id The id which will be used to report back changes to the element
    @param dotNetObject The reference to the caller which should be notified on a resize of the element
    @param includeStyle When set to true, computed style changes are also be reported
    */
    public observe = (
        elementReference: HTMLElement | undefined,
        id: string | undefined,
        dotNetObject: DotNet.DotNetObject,
        includeStyle: boolean
    ): void => {
        if (id === undefined || id === '' || elementReference?.dataset === undefined)
            return;

        elementReference.dataset.observerId = id;
        this.#observers.set(id, new ObserverDetails(dotNetObject, includeStyle));

        this.#resizeObserver.observe(elementReference);
    };

    /**
     Removes an element from the list of observed elements via it's reference.
     @param elementRef The reference to the element which should no be observed
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
 Creates and returns a new instance
 @returns The created observer
 */
export function createInstance(): HtmlElementResizeObserver {
    return new HtmlElementResizeObserver();
}

class ObserverDetails {
    constructor(readonly dotNetObject: DotNet.DotNetObject, readonly includeStyle: boolean) {}
}
