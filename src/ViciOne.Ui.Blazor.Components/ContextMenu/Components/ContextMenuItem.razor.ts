import { MouseLeaveDirection } from '/_content/ViciOne.Ui.Blazor.Components/context-menu/enums/mouse-leave-direction.js';
import { ChildContextMenuPosition } from '/_content/ViciOne.Ui.Blazor.Components/context-menu/models/child-context-menu-position.js';

class ContextMenuItem {
    private readonly bodyMouseOverListenerBinding: ((e: MouseEvent) => void);
    private readonly htmlElementMouseLeaveListenerBinding: ((e: MouseEvent) => void);

    private htmlElementObservedForMouseLeave: HTMLElement | undefined = undefined;

    constructor(readonly dotNetObject: DotNet.DotNetObject) {
        this.bodyMouseOverListenerBinding = this.bodyMouseOverListener.bind(this);
        this.htmlElementMouseLeaveListenerBinding = this.htmlElementMouseLeaveListener.bind(this);
    }

    public calculateChildContextMenuPosition(htmlElement: HTMLElement, childContextMenuHtmlElement: HTMLElement) {
        const { visualViewport } = window;
        if (!visualViewport) {
            console.error('Visual Viewport API missing');

            return null;
        }

        if (!htmlElement) {
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

    public startObserveMouseLeave(htmlElement: HTMLElement) {
        if (!htmlElement)
            return;

        if (htmlElement === this.htmlElementObservedForMouseLeave)
            return;

        if (this.htmlElementObservedForMouseLeave)
            this.htmlElementObservedForMouseLeave.removeEventListener('mouseleave', this.htmlElementMouseLeaveListenerBinding);

        this.htmlElementObservedForMouseLeave = htmlElement;
        this.htmlElementObservedForMouseLeave.addEventListener('mouseleave', this.htmlElementMouseLeaveListenerBinding);

        document.body.removeEventListener('mouseover', this.bodyMouseOverListenerBinding);
        document.body.addEventListener('mouseover', this.bodyMouseOverListenerBinding);
    }

    public endObserveMouseLeave() {
        document.body.removeEventListener('mouseover', this.bodyMouseOverListenerBinding);

        if (this.htmlElementObservedForMouseLeave) {
            this.htmlElementObservedForMouseLeave.removeEventListener('mouseleave', this.htmlElementMouseLeaveListenerBinding);

            this.htmlElementObservedForMouseLeave = undefined;
        }
    }

    public dispose() {
        if (this.htmlElementObservedForMouseLeave !== undefined)
            this.endObserveMouseLeave();
    }

    private async htmlElementMouseLeaveListener(e: MouseEvent) {
        if (this.htmlElementObservedForMouseLeave) {
            const mouseLeaveDirection = this.getMouseLeaveDirection(this.htmlElementObservedForMouseLeave, e);

            if (mouseLeaveDirection !== undefined)
                await this.dotNetObject.invokeMethodAsync('MouseLeaveAsync', mouseLeaveDirection);
        }
    }

    private getMouseLeaveDirection(htmlElement: HTMLElement, e: MouseEvent): MouseLeaveDirection | undefined {
        if (!htmlElement)
            return undefined;

        const x = e.clientX;
        const y = e.clientY;

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

    private async bodyMouseOverListener(e: MouseEvent) {
        if (!this.htmlElementObservedForMouseLeave)
            return;

        if (e.target === this.htmlElementObservedForMouseLeave)
            return;

        if (!(e.target instanceof HTMLElement))
            return;

        // Traverse through the ancestors of the element where a mouse over was detected ...
        let ancestor = e.target.parentElement;
        while (ancestor
            && ancestor !== this.htmlElementObservedForMouseLeave // ... as long as the we don't find our html element
        )
            ancestor = ancestor.parentElement;

        // If the resulting ancestor is null ...
        if (!ancestor) {
            // ... then the element does not belong to our context menu and we have to notify the mouse leave

            const mouseLeaveDirection = this.getMouseLeaveDirection(this.htmlElementObservedForMouseLeave, e);

            if (mouseLeaveDirection !== undefined)
                await this.dotNetObject.invokeMethodAsync('MouseLeaveAsync', mouseLeaveDirection);
        }
    }
}

export async function attach(dotNetObject: DotNet.DotNetObject) {
    const contextMenuItem = new ContextMenuItem(dotNetObject);

    return contextMenuItem;
}
