import { PointerCapture } from '/_content/ViciOne.Ui.Blazor.Components/js/pointer-capture.js';
import '/_content/ViciOne.Ui.Blazor.Components/js/pointer-event-mixins.js';

class MoveInteraction {
    private readonly pointerDownEventListenerBinding: (e: PointerEvent) => void;
    private readonly pointerMoveEventListenerBinding: (e: PointerEvent) => void;
    private readonly pointerUpEventListenerBinding: (e: PointerEvent) => void;

    // MoveContainer should not have any border / padding because PointerCapture is based on bounding client rects
    // to avoid rounding errors!
    constructor(readonly moveableId: string, readonly moveable: HTMLElement, readonly moveHandle: HTMLElement,
        readonly moveContainer: HTMLElement, readonly startedCssClass: string, readonly endedCssClass: string,
        readonly dotNetObject: DotNet.DotNetObject) {

        this.pointerDownEventListenerBinding = this.pointerDownEventListener.bind(this);
        this.pointerMoveEventListenerBinding = this.pointerMoveEventListener.bind(this);
        this.pointerUpEventListenerBinding = this.pointerUpEventListener.bind(this);

        this.moveHandle.addEventListener('pointerdown', this.pointerDownEventListenerBinding);
        this.moveHandle.addEventListener('pointerup', this.pointerUpEventListenerBinding);
    }

    public dispose() {
        this.moveHandle.removeEventListener('pointerup', this.pointerUpEventListenerBinding);
        this.moveHandle.removeEventListener('pointermove', this.pointerMoveEventListenerBinding);
        this.moveHandle.removeEventListener('pointerdown', this.pointerDownEventListenerBinding);
    }

    private hasRequiredKeyState(e: PointerEvent) {
        return !e.altKey && !e.ctrlKey && !e.shiftKey; // No modifier key should be pressed as these are reserved for other interactions
    }

    private pointerDownEventListener(e: PointerEvent) {
        if (!this.hasRequiredKeyState(e))
            return;

        if (e.isRaisedByNestableOf(this.moveHandle, 'moveable'))
            return; // Do nothing as nested moveable has its own handler

        // Start interaction when mouse is moved
        this.moveHandle.addEventListener('pointermove', this.pointerMoveEventListenerBinding, { once: true });
    }

    private pointerMoveEventListener(e: PointerEvent) {
        if (!this.hasRequiredKeyState(e))
            return;

        const moveContainerBoundingClientRect = this.moveContainer.getBoundingClientRect();

        const pointerCapture = new PointerCapture();
        pointerCapture.onPointerUp = this.moveablePointerUp.bind(this);
        pointerCapture.startedCssClass = this.startedCssClass;
        pointerCapture.endedCssClass = this.endedCssClass;
        pointerCapture.start(e, this.moveable, moveContainerBoundingClientRect);
    }

    private pointerUpEventListener(_e: PointerEvent) {
        this.moveHandle.removeEventListener('pointermove', this.pointerMoveEventListenerBinding);
    }

    private async moveablePointerUp(_moveable: HTMLElement, x: number, y: number) {
        await this.dotNetObject.invokeMethodAsync('OnMoveablePointerUp', this.moveableId, x, y);
    }
}

export async function attach(moveableId: string, moveable: HTMLElement, moveHandle: HTMLElement,
    moveContainer: HTMLElement, startedCssClass: string, endedCssClass: string, dotNetObject: DotNet.DotNetObject) {

    const moveInteraction = new MoveInteraction(moveableId, moveable, moveHandle, moveContainer, startedCssClass, endedCssClass, dotNetObject);

    return moveInteraction;
}
