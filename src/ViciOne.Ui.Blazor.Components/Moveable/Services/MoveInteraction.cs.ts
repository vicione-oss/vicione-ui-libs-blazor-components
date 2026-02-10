import { PointerCapture } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture.js';
import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { type MoveInteractionContext } from '/_content/ViciOne.Ui.Blazor.Components/moveable/move-interaction-context.js';
import '/_content/ViciOne.Ui.Blazor.Components/js/pointer-event-mixins.js';

class MoveInteraction {
    private readonly pointerDownEventListenerBinding: (e: PointerEvent) => void;
    private readonly pointerMoveEventListenerBinding: (e: PointerEvent) => void;
    private readonly pointerUpEventListenerBinding: (e: PointerEvent) => void;
    private readonly pointerCaptureBehaviors: Set<PointerCaptureBehavior> = new Set<PointerCaptureBehavior>();

    // MoveContainer should not have any border / padding because PointerCapture is based on bounding client rects
    // to avoid rounding errors!
    constructor(readonly context: MoveInteractionContext) {
        this.pointerDownEventListenerBinding = this.pointerDownEventListener.bind(this);
        this.pointerMoveEventListenerBinding = this.pointerMoveEventListener.bind(this);
        this.pointerUpEventListenerBinding = this.pointerUpEventListener.bind(this);

        this.context.pointerCaptureBehaviors?.forEach(b => this.pointerCaptureBehaviors.add(b));

        this.context.moveHandle.addEventListener('pointerdown', this.pointerDownEventListenerBinding);
        this.context.moveHandle.addEventListener('pointerup', this.pointerUpEventListenerBinding);
    }

    public dispose() {
        this.context.moveHandle.removeEventListener('pointerup', this.pointerUpEventListenerBinding);
        this.context.moveHandle.removeEventListener('pointermove', this.pointerMoveEventListenerBinding);
        this.context.moveHandle.removeEventListener('pointerdown', this.pointerDownEventListenerBinding);

        this.pointerCaptureBehaviors.clear();
    }

    public addPointerCaptureBehavior(pointerCaptureBehavior: PointerCaptureBehavior) {
        this.pointerCaptureBehaviors.add(pointerCaptureBehavior);
    }

    public removePointerCaptureBehavior(pointerCaptureBehavior: PointerCaptureBehavior) {
        this.pointerCaptureBehaviors.delete(pointerCaptureBehavior);
    }

    private pointerDownEventListener(e: PointerEvent) {
        if (e.isModifierKeyPressed())
            return;

        if (e.isRaisedByNestableOf(this.context.moveHandle, 'moveable'))
            return; // Do nothing as nested moveable has its own handler

        // Start interaction when mouse is moved
        this.context.moveHandle.addEventListener('pointermove', this.pointerMoveEventListenerBinding, { once: true });
    }

    private pointerMoveEventListener(e: PointerEvent) {
        if (e.isModifierKeyPressed())
            return;

        const moveContainerBoundingClientRect = this.context.moveContainer.getBoundingClientRect();

        const pointerCapture = new PointerCapture();
        pointerCapture.onPointerUp = this.moveablePointerUp.bind(this);
        pointerCapture.startedCssClass = this.context.startedCssClass;
        pointerCapture.endedCssClass = this.context.endedCssClass;

        if (this.pointerCaptureBehaviors.size > 0)
            pointerCapture.behaviors = [...this.pointerCaptureBehaviors];
        else
            pointerCapture.behaviors = undefined;

        pointerCapture.start(e, this.context.moveable, moveContainerBoundingClientRect);
    }

    private pointerUpEventListener(_e: PointerEvent) {
        this.context.moveHandle.removeEventListener('pointermove', this.pointerMoveEventListenerBinding);
    }

    private async moveablePointerUp(_moveable: HTMLElement, x: number, y: number) {
        await this.context.dotNetObject.invokeMethodAsync('OnMoveablePointerUpAsync', this.context.moveableId, x, y);
    }
}

export async function attach(context: MoveInteractionContext) {
    const moveInteraction = new MoveInteraction(context);

    return moveInteraction;
}
