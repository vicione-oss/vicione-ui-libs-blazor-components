import { PointerCapture } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture.js';
import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { type CaptureTarget } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/capture-target.js';
import { AggregatePointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/aggregate-pointer-capture-behavior.js';
import { MovePointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/move-pointer-capture-behavior.js';
import { SetPositionPointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/set-position-pointer-capture-behavior.js';
import { type MoveInteractionContext } from '/_content/ViciOne.Ui.Blazor.Components/moveable/move-interaction-context.js';
import '/_content/ViciOne.Ui.Blazor.Components/js/pointer-event-mixins.js';

class MoveInteraction {
    readonly #movePointerCaptureBehavior = new MovePointerCaptureBehavior();
    readonly #setPositionPointerCaptureBehavior = new SetPositionPointerCaptureBehavior();
    readonly #additionalPointerCaptureBehaviors = new Set<PointerCaptureBehavior>();

    // MoveContainer should not have any border / padding because PointerCapture is based on bounding client rects
    // to avoid rounding errors!
    constructor(readonly context: MoveInteractionContext) {
        this.context.pointerCaptureBehaviors?.forEach(b => this.#additionalPointerCaptureBehaviors.add(b));

        this.context.moveHandle.addEventListener('pointerdown', this.#pointerDownEventListener);
        this.context.moveHandle.addEventListener('pointerup', this.#pointerUpEventListener);
    }

    public dispose() {
        this.context.moveHandle.removeEventListener('pointerup', this.#pointerUpEventListener);
        this.context.moveHandle.removeEventListener('pointermove', this.#pointerMoveEventListener);
        this.context.moveHandle.removeEventListener('pointerdown', this.#pointerDownEventListener);

        this.#additionalPointerCaptureBehaviors.clear();
    }

    public addPointerCaptureBehavior(pointerCaptureBehavior: PointerCaptureBehavior) {
        this.#additionalPointerCaptureBehaviors.add(pointerCaptureBehavior);
    }

    public removePointerCaptureBehavior(pointerCaptureBehavior: PointerCaptureBehavior) {
        this.#additionalPointerCaptureBehaviors.delete(pointerCaptureBehavior);
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
        pointerCapture.ongoingCssClass = this.context.ongoingCssClass;
        pointerCapture.endedCssClass = this.context.endedCssClass;
        pointerCapture.behaviors = [...this.#getAllPointerCaptureBehaviors(moveContainerBoundingClientRect)];

        pointerCapture.start(e, this.context.moveable, moveContainerBoundingClientRect);
    };

    * #getAllPointerCaptureBehaviors(moveContainerBoundingClientRect: DOMRect): IterableIterator<PointerCaptureBehavior> {
        this.#movePointerCaptureBehavior.initialize(moveContainerBoundingClientRect);
        yield this.#movePointerCaptureBehavior;

        if (this.#additionalPointerCaptureBehaviors.size > 0)
            yield new AggregatePointerCaptureBehavior(this.#additionalPointerCaptureBehaviors);

        yield this.#setPositionPointerCaptureBehavior;
    }

    readonly #pointerUpEventListener = (_e: PointerEvent) => {
        this.context.moveHandle.removeEventListener('pointermove', this.#pointerMoveEventListener);
    };

    readonly #moveablePointerUp = async (captureTarget: CaptureTarget, pointerMoved: boolean) => {
        if (pointerMoved)
            await this.context.dotNetObject.invokeMethodAsync('MoveablePointerUpAsync', this.context.moveableId, captureTarget.rect.left, captureTarget.rect.top);
    };
}

export async function attach(context: MoveInteractionContext) {
    const moveInteraction = new MoveInteraction(context);

    return moveInteraction;
}
