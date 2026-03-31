import { ContextMenuPosition } from '/_content/ViciOne.Ui.Blazor.Components/context-menu/models/context-menu-position.js';

class ContextMenu {
    readonly #dotNetObject: DotNet.DotNetObject;

    constructor(dotNetObject: DotNet.DotNetObject) {
        this.#dotNetObject = dotNetObject;
    }

    /**
     * Returns a position based on the given mouse event that ensures the context menu is fully visible
     */
    public calculatePosition(htmlElement: HTMLElement, mouseEvent: MouseEvent) {
        if (!htmlElement) {
            console.error('ContextMenu.calculatePosition() -> HTML element is undefined');
            return null;
        }

        const boundingClientRect = htmlElement.getBoundingClientRect();

        const { visualViewport } = window;
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

    #addPointerDownEventListener() {
        window.addEventListener('pointerdown', this.#windowPointerDownEventListener);
    }

    #removePointerDownEventListener() {
        window.removeEventListener('pointerdown', this.#windowPointerDownEventListener);
    }

    readonly #windowPointerDownEventListener = async (e: PointerEvent) => {
        const contextMenuHtmlElements = await this.#dotNetObject.invokeMethodAsync<HTMLElement[]>('GetVisibleContextMenuHtmlElementsAsync');

        let clickedInsideAnyContextMenu = false;

        for (const contextMenuHtmlElement of contextMenuHtmlElements) {
            const x = e.clientX;
            const y = e.clientY;

            const boundingClientRect = contextMenuHtmlElement.getBoundingClientRect();

            const boundingClientRectAdjusted = new DOMRect(boundingClientRect.x + window.scrollX,
                boundingClientRect.y + window.scrollY,
                boundingClientRect.width,
                boundingClientRect.height);

            const clickedInsideContextMenu = boundingClientRectAdjusted.x <= x && x < boundingClientRectAdjusted.right
                && boundingClientRectAdjusted.y <= y && y < boundingClientRectAdjusted.bottom;

            if (clickedInsideContextMenu) {
                clickedInsideAnyContextMenu = true;

                break;
            }
        }

        if (!clickedInsideAnyContextMenu)
            await this.#dotNetObject.invokeMethodAsync('CloseAsync');
    };
}

export async function attach(dotNetObject: DotNet.DotNetObject) {
    const contextMenu = new ContextMenu(dotNetObject);

    return contextMenu;
}
