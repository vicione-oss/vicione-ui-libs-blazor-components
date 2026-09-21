import { PointerCapture } from '../../PointerCapture/Scripts/PointerCapture.ts';
import { type PointerCaptureBehavior } from '../../PointerCapture/Scripts/PointerCaptureBehavior.ts';
import { type CaptureTarget } from '../../PointerCapture/Scripts/CaptureTarget.ts';
import { AggregatePointerCaptureBehavior } from '../../PointerCapture/Scripts/AggregatePointerCaptureBehavior.ts';
import { MovePointerCaptureBehavior } from '../../PointerCapture/Scripts/MovePointerCaptureBehavior.ts';
import { SetPositionPointerCaptureBehavior } from '../../PointerCapture/Scripts/SetPositionPointerCaptureBehavior.ts';
import { type MoveInteractionContext } from '../Models/MoveInteractionContext.cs.ts';
import '../../Scripts/PointerEventMixins.ts';

export class MoveInteraction {
    readonly #movePointerCaptureBehavior = new MovePointerCaptureBehavior();
    readonly #setPositionPointerCaptureBehavior = new SetPositionPointerCaptureBehavior();
    readonly #additionalPointerCaptureBehaviors = new Set<PointerCaptureBehavior>();

    readonly #pointerDownEventListener = (event: PointerEvent) => {
        if (event.isModifierKeyPressed())
            return;

        if (event.isRaisedByElementWithOwnHandlerNestedIn(this.context.moveHandle, 'moveable'))
            return; // Do nothing as nested moveable has its own handler

        // Start interaction when mouse is moved
        this.context.moveHandle.addEventListener('pointermove', this.#pointerMoveEventListener, { once: true });
    };

    readonly #pointerMoveEventListener = (event: PointerEvent) => {
        if (event.isModifierKeyPressed())
            return;

        const moveContainerBoundingClientRect = this.context.moveContainer.getBoundingClientRect();

        const pointerCapture = new PointerCapture();
        pointerCapture.onPointerUp = this.#moveablePointerUp;
        pointerCapture.startedCssClass = this.context.startedCssClass;
        pointerCapture.ongoingCssClass = this.context.ongoingCssClass;
        pointerCapture.endedCssClass = this.context.endedCssClass;
        pointerCapture.behaviors = [...this.#getAllPointerCaptureBehaviors(moveContainerBoundingClientRect)];

        pointerCapture.start(event, this.context.moveable, moveContainerBoundingClientRect);
    };

    readonly #pointerUpEventListener = (_event: PointerEvent) => {
        this.context.moveHandle.removeEventListener('pointermove', this.#pointerMoveEventListener);
    };

    readonly #moveablePointerUp = (captureTarget: CaptureTarget, wasPointerMoved: boolean): void => {
        void this.#handleMoveablePointerUp(captureTarget, wasPointerMoved);
    };

    readonly #handleMoveablePointerUp = async (captureTarget: CaptureTarget, wasPointerMoved: boolean) => {
        if (wasPointerMoved) {
            await this.context.dotNetObject.invokeMethodAsync('MoveablePointerUpAsync',
                this.context.moveableId,
                captureTarget.rect.left,
                captureTarget.rect.top);
        }
    };

    // MoveContainer should not have any border / padding because PointerCapture is based on bounding client rects
    // to avoid rounding errors!
    constructor(readonly context: MoveInteractionContext) {
        for (const behavior of this.context.pointerCaptureBehaviors ?? [])
            this.#additionalPointerCaptureBehaviors.add(behavior);

        this.context.moveHandle.addEventListener('pointerdown', this.#pointerDownEventListener);
        this.context.moveHandle.addEventListener('pointerup', this.#pointerUpEventListener);
    }

    * #getAllPointerCaptureBehaviors(moveContainerBoundingClientRect: DOMRect): IterableIterator<PointerCaptureBehavior> {
        this.#movePointerCaptureBehavior.initialize(moveContainerBoundingClientRect);
        yield this.#movePointerCaptureBehavior;

        if (this.#additionalPointerCaptureBehaviors.size > 0)
            yield new AggregatePointerCaptureBehavior(this.#additionalPointerCaptureBehaviors);

        yield this.#setPositionPointerCaptureBehavior;
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
}
