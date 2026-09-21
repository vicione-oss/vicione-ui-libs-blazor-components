import { MouseLeaveDirection } from '../Enums/MouseLeaveDirection.cs.ts';
import { ChildContextMenuPosition } from '../Models/ChildContextMenuPosition.cs.ts';

export class ContextMenuItem {
    readonly #dotNetObject: DotNet.DotNetObject;
    #htmlElementObservedForMouseLeave: HTMLElement | undefined = undefined;

    readonly #htmlElementMouseLeaveListener = (event: MouseEvent): void => {
        void this.#handleHtmlElementMouseLeave(event);
    };

    readonly #handleHtmlElementMouseLeave = async (event: MouseEvent) => {
        if (!this.#htmlElementObservedForMouseLeave)
            return;

        const mouseLeaveDirection = this.#getMouseLeaveDirection(this.#htmlElementObservedForMouseLeave, event);

        if (mouseLeaveDirection !== undefined)
            await this.#dotNetObject.invokeMethodAsync('MouseLeaveAsync', mouseLeaveDirection);
    };

    readonly #bodyMouseOverListener = (event: MouseEvent): void => {
        void this.#handleBodyMouseOver(event);
    };

    readonly #handleBodyMouseOver = async (event: MouseEvent) => {
        if (!this.#htmlElementObservedForMouseLeave)
            return;

        if (event.target === this.#htmlElementObservedForMouseLeave)
            return;

        if (!(event.target instanceof HTMLElement))
            return;

        // Traverse through the ancestors of the element where a mouse over was detected ...
        let ancestor = event.target.parentElement;
        while (ancestor &&
            ancestor !== this.#htmlElementObservedForMouseLeave // ... as long as the we don't find our html element
        )
            ancestor = ancestor.parentElement;

        // If the resulting ancestor is null ...
        if (!ancestor) {
            // ... then the element does not belong to our context menu and we have to notify the mouse leave

            const mouseLeaveDirection = this.#getMouseLeaveDirection(this.#htmlElementObservedForMouseLeave, event);

            if (mouseLeaveDirection !== undefined)
                await this.#dotNetObject.invokeMethodAsync('MouseLeaveAsync', mouseLeaveDirection);
        }
    };

    constructor(dotNetObject: DotNet.DotNetObject) {
        this.#dotNetObject = dotNetObject;
    }

    #getMouseLeaveDirection(htmlElement: HTMLElement | undefined, event: MouseEvent): MouseLeaveDirection | undefined {
        if (htmlElement === undefined)
            return undefined;

        const x = event.clientX;
        const y = event.clientY;

        const boundingClientRect = htmlElement.getBoundingClientRect();

        const boundingClientRectAdjusted = new DOMRect(boundingClientRect.x + window.scrollX,
            boundingClientRect.y + window.scrollY,
            boundingClientRect.width,
            boundingClientRect.height);

        let mouseLeaveDirection: MouseLeaveDirection | undefined;

        if (x < boundingClientRectAdjusted.x)
            mouseLeaveDirection = MouseLeaveDirection.Left;
        else if (x > boundingClientRectAdjusted.right)
            mouseLeaveDirection = MouseLeaveDirection.Right;
        else if (y < boundingClientRectAdjusted.y)
            mouseLeaveDirection = MouseLeaveDirection.Top;
        else if (y > boundingClientRectAdjusted.bottom)
            mouseLeaveDirection = MouseLeaveDirection.Bottom;

        return mouseLeaveDirection;
    }

    public calculateChildContextMenuPosition(htmlElement: HTMLElement | undefined, childContextMenuHtmlElement: HTMLElement) {
        const { visualViewport } = globalThis;
        if (!visualViewport) {
            console.error('Visual Viewport API missing');

            return null;
        }

        if (htmlElement === undefined) {
            console.error('ContextMenuItem.calculateChildContextMenuPosition() -> HTML element is undefined');
            return null;
        }

        const { pageLeft } = visualViewport;
        const pageRight = visualViewport.pageLeft + visualViewport.width;

        const boundingClientRect = htmlElement.getBoundingClientRect();
        const childContextMenuBoundingClientRect = childContextMenuHtmlElement.getBoundingClientRect();
        const childContextMenuWidth = childContextMenuBoundingClientRect.width;

        const boundingClientRectAdjusted = new DOMRect(boundingClientRect.x + window.scrollX,
            boundingClientRect.y + window.scrollY,
            boundingClientRect.width,
            boundingClientRect.height);

        let mouseLeaveDirection = MouseLeaveDirection.Right;
        let x = boundingClientRectAdjusted.right;

        // If child context menu would exceed right bound of visual viewport ...
        if (x + childContextMenuWidth > pageRight) {
            // ... try to position it on the left side ...
            x = boundingClientRectAdjusted.left - childContextMenuWidth;
            mouseLeaveDirection = MouseLeaveDirection.Left;

            // ... and if it now would exceed left bound of visual viewport ...
            if (x < pageLeft) {
                x = boundingClientRectAdjusted.right; // ... rever back to original position
                mouseLeaveDirection = MouseLeaveDirection.Right;
            }
        }

        const y = boundingClientRectAdjusted.top;

        return new ChildContextMenuPosition(Math.trunc(x), Math.trunc(y), mouseLeaveDirection);
    }

    public startObserveMouseLeave(htmlElement: HTMLElement | undefined) {
        if (htmlElement === undefined)
            return;

        if (htmlElement === this.#htmlElementObservedForMouseLeave)
            return;

        if (this.#htmlElementObservedForMouseLeave)
            this.#htmlElementObservedForMouseLeave.removeEventListener('mouseleave', this.#htmlElementMouseLeaveListener);

        this.#htmlElementObservedForMouseLeave = htmlElement;
        this.#htmlElementObservedForMouseLeave.addEventListener('mouseleave', this.#htmlElementMouseLeaveListener);

        document.body.removeEventListener('mouseover', this.#bodyMouseOverListener);
        document.body.addEventListener('mouseover', this.#bodyMouseOverListener);
    }

    public endObserveMouseLeave() {
        document.body.removeEventListener('mouseover', this.#bodyMouseOverListener);

        if (this.#htmlElementObservedForMouseLeave) {
            this.#htmlElementObservedForMouseLeave.removeEventListener('mouseleave', this.#htmlElementMouseLeaveListener);

            this.#htmlElementObservedForMouseLeave = undefined;
        }
    }

    public dispose() {
        if (this.#htmlElementObservedForMouseLeave !== undefined)
            this.endObserveMouseLeave();
    }
}
