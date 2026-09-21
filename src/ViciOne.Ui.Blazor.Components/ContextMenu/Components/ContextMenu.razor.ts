import { ContextMenuPosition } from '../Models/ContextMenuPosition.cs.ts';

export class ContextMenu {
    readonly #dotNetObject: DotNet.DotNetObject;

    readonly #windowPointerDownEventListener = (event: PointerEvent): void => {
        void this.#handleWindowPointerDown(event);
    };

    readonly #handleWindowPointerDown = async (event: PointerEvent) => {
        const contextMenuHtmlElements = await this.#dotNetObject.invokeMethodAsync<HTMLElement[]>('GetVisibleContextMenuHtmlElements');

        let wasClickedInsideAnyContextMenu = false;

        for (const contextMenuHtmlElement of contextMenuHtmlElements) {
            const x = event.clientX;
            const y = event.clientY;

            const boundingClientRect = contextMenuHtmlElement.getBoundingClientRect();

            const boundingClientRectAdjusted = new DOMRect(boundingClientRect.x + window.scrollX,
                boundingClientRect.y + window.scrollY,
                boundingClientRect.width,
                boundingClientRect.height);

            const wasClickedInsideContextMenu = boundingClientRectAdjusted.x <= x && x < boundingClientRectAdjusted.right &&
                boundingClientRectAdjusted.y <= y && y < boundingClientRectAdjusted.bottom;

            if (wasClickedInsideContextMenu) {
                wasClickedInsideAnyContextMenu = true;

                break;
            }
        }

        if (!wasClickedInsideAnyContextMenu)
            await this.#dotNetObject.invokeMethodAsync('CloseAsync');
    };

    constructor(dotNetObject: DotNet.DotNetObject) {
        this.#dotNetObject = dotNetObject;
    }

    #addPointerDownEventListener() {
        globalThis.addEventListener('pointerdown', this.#windowPointerDownEventListener);
    }

    #removePointerDownEventListener() {
        globalThis.removeEventListener('pointerdown', this.#windowPointerDownEventListener);
    }

    /**
     Returns a position based on the given mouse event that ensures the context menu is fully visible
     */
    public calculatePosition(htmlElement: HTMLElement | undefined, mouseEvent: MouseEvent) {
        if (htmlElement === undefined) {
            console.error('ContextMenu.calculatePosition() -> HTML element is undefined');
            return null;
        }

        const boundingClientRect = htmlElement.getBoundingClientRect();

        const { visualViewport } = globalThis;
        if (!visualViewport) {
            console.error('Visual Viewport API missing');

            return null;
        }

        const pageRight = visualViewport.pageLeft + visualViewport.width;
        const pageBottom = visualViewport.pageTop + visualViewport.height;

        let x = mouseEvent.pageX;
        if (x + boundingClientRect.width > pageRight)
            x = Math.trunc(pageRight - boundingClientRect.width);

        let y = mouseEvent.pageY;
        if (y + boundingClientRect.height > pageBottom)
            y = Math.trunc(pageBottom - boundingClientRect.height);

        return new ContextMenuPosition(x, y);
    }

    public startObserveWindowPointerDown() {
        this.#removePointerDownEventListener();
        this.#addPointerDownEventListener();
    }

    public endObserveWindowPointerDown() {
        this.#removePointerDownEventListener();
    }

    public dispose() {
        this.endObserveWindowPointerDown();
    }
}
