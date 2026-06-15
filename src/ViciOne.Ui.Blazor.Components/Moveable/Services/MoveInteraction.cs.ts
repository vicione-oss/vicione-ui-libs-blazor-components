import { PointerCapture } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture.js';
import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { type MoveInteractionContext } from '/_content/ViciOne.Ui.Blazor.Components/moveable/move-interaction-context.js';
import '/_content/ViciOne.Ui.Blazor.Components/js/pointer-event-mixins.js';

class MoveInteraction {
    readonly #pointerCaptureBehaviors: Set<PointerCaptureBehavior> = new Set<PointerCaptureBehavior>();

    // MoveContainer should not have any border / padding because PointerCapture is based on bounding client rects
    // to avoid rounding errors!
    constructor(readonly context: MoveInteractionContext) {
        this.context.pointerCaptureBehaviors?.forEach(b => this.#pointerCaptureBehaviors.add(b));

        this.context.moveHandle.addEventListener('pointerdown', this.#pointerDownEventListener);
        this.context.moveHandle.addEventListener('pointerup', this.#pointerUpEventListener);
    }

    public dispose() {
        this.context.moveHandle.removeEventListener('pointerup', this.#pointerUpEventListener);
        this.context.moveHandle.removeEventListener('pointermove', this.#pointerMoveEventListener);
        this.context.moveHandle.removeEventListener('pointerdown', this.#pointerDownEventListener);

        this.#pointerCaptureBehaviors.clear();
    }

    public addPointerCaptureBehavior(pointerCaptureBehavior: PointerCaptureBehavior) {
        this.#pointerCaptureBehaviors.add(pointerCaptureBehavior);
    }

    public removePointerCaptureBehavior(pointerCaptureBehavior: PointerCaptureBehavior) {
        this.#pointerCaptureBehaviors.delete(pointerCaptureBehavior);
    }

    readonly #pointerDownEventListener = (e: PointerEvent) => {
        if (e.isModifierKeyPressed())
            return;

        if (e.isRaisedByElementWithOwnHandlerNestedIn(this.context.moveHandle, 'moveable'))
            return; // Do nothing as nested moveable has its own handler

        // Start interaction when mouse is moved
        this.context.moveHandle.addEventListener('pointermove', this.#pointerMoveEventListener, { once: true });
    };

    readonly #pointerMoveEventListener = (e: PointerEvent) => {
        if (e.isModifierKeyPressed())
            return;

        const moveContainerBoundingClientRect = this.context.moveContainer.getBoundingClientRect();

        const pointerCapture = new PointerCapture();
        pointerCapture.onPointerUp = this.#moveablePointerUp;
        pointerCapture.startedCssClass = this.context.startedCssClass;
        pointerCapture.endedCssClass = this.context.endedCssClass;

        if (this.#pointerCaptureBehaviors.size > 0)
            pointerCapture.behaviors = [...this.#pointerCaptureBehaviors];
        else
            pointerCapture.behaviors = undefined;

        pointerCapture.start(e, this.context.moveable, moveContainerBoundingClientRect);
    };

    readonly #pointerUpEventListener = (_e: PointerEvent) => {
        this.context.moveHandle.removeEventListener('pointermove', this.#pointerMoveEventListener);
    };

    readonly #moveablePointerUp = async (_moveable: HTMLElement, x: number, y: number) => {
        await this.context.dotNetObject.invokeMethodAsync('MoveablePointerUpAsync', this.context.moveableId, x, y);
    };
}

export async function attach(context: MoveInteractionContext) {
    const moveInteraction = new MoveInteraction(context);

    return moveInteraction;
}
