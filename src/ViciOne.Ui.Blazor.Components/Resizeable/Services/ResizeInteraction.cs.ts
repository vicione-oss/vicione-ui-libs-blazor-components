import { PointerCapture } from '../../PointerCapture/Scripts/PointerCapture.ts';
import { type CaptureTarget } from '../../PointerCapture/Scripts/CaptureTarget.ts';
import { type PointerCaptureBehavior } from '../../PointerCapture/Scripts/PointerCaptureBehavior.ts';
import { type ResizeInteractionContext } from '../Models/ResizeInteractionContext.cs.ts';
import { AggregatePointerCaptureBehavior } from '../../PointerCapture/Scripts/AggregatePointerCaptureBehavior.ts';
import { ResetRectPointerCaptureBehavior } from '../../PointerCapture/Scripts/ResetRectPointerCaptureBehavior.ts';
import { ResizeLeftPointerCaptureBehavior } from '../Scripts/ResizeLeftPointerCaptureBehavior.ts';
import { ResizeRightPointerCaptureBehavior } from '../Scripts/ResizeRightPointerCaptureBehavior.ts';
import { ResizeUpPointerCaptureBehavior } from '../Scripts/ResizeUpPointerCaptureBehavior.ts';
import { ResizeDownPointerCaptureBehavior } from '../Scripts/ResizeDownPointerCaptureBehavior.ts';
import { SetPositionPointerCaptureBehavior } from '../../PointerCapture/Scripts/SetPositionPointerCaptureBehavior.ts';
import { SetSizePointerCaptureBehavior } from '../../PointerCapture/Scripts/SetSizePointerCaptureBehavior.ts';
import { type ResizePointerCaptureBehavior } from '../Scripts/ResizePointerCaptureBehavior.ts';
import { type ResizePointerCaptureBehaviorInitializeArgs } from '../Scripts/ResizePointerCaptureBehaviorInitializeArgs.ts';
import { type ResizeHandlePosition } from '../Enums/ResizeHandlePosition.cs.ts';
import '../../Scripts/PointerEventMixins.ts';

export class ResizeInteraction {
    readonly #resetRectPointerCaptureBehavior = new ResetRectPointerCaptureBehavior();
    readonly #setPositionPointerCaptureBehavior = new SetPositionPointerCaptureBehavior();
    readonly #setSizePointerCaptureBehavior = new SetSizePointerCaptureBehavior();

    readonly #resizePointerCaptureBehaviors: ResizePointerCaptureBehavior[] = [
        new ResizeUpPointerCaptureBehavior(),
        new ResizeDownPointerCaptureBehavior(),
        new ResizeRightPointerCaptureBehavior(),
        new ResizeLeftPointerCaptureBehavior()
    ];

    readonly #additionalPointerCaptureBehaviors: Set<PointerCaptureBehavior> = new Set<PointerCaptureBehavior>();

    readonly #pointerDownEventListener = (event: PointerEvent) => {
        if (event.isModifierKeyPressed())
            return;

        const { currentTarget } = event;
        const resizeHandleInfo = this.context.resizeHandles.find(h => h.element === currentTarget);

        if (resizeHandleInfo === undefined)
            return;

        const resizeContainerBoundingClientRect = this.context.resizeContainer.getBoundingClientRect();

        const pointerCapture = new PointerCapture();
        pointerCapture.startedCssClass = this.context.startedCssClass;
        pointerCapture.ongoingCssClass = this.context.ongoingCssClass;
        pointerCapture.endedCssClass = this.context.endedCssClass;
        pointerCapture.behaviors = [...this.#getAllPointerCaptureBehaviors(resizeHandleInfo.position, resizeContainerBoundingClientRect)];

        pointerCapture.onPointerUp = (captureTarget, pointerMoved): void => {
            void this.#handleResizePointerUp(captureTarget, pointerMoved);
        };

        pointerCapture.start(event, this.context.resizeable, resizeContainerBoundingClientRect);
    };

    readonly #handleResizePointerUp = async (captureTarget: CaptureTarget, hasPointerMoved: boolean) => {
        if (!hasPointerMoved)
            return;

        const domRect = captureTarget.rect.toDomRect();

        await this.context.dotNetObject.invokeMethodAsync('OnUpdatePositionAndSizeAsync', this.context.resizeableId, domRect);
    };

    constructor(readonly context: ResizeInteractionContext) {
        for (const behavior of this.context.pointerCaptureBehaviors ?? [])
            this.#additionalPointerCaptureBehaviors.add(behavior);

        for (const resizeHandleInfo of this.context.resizeHandles)
            resizeHandleInfo.element.addEventListener('pointerdown', this.#pointerDownEventListener);

    }

    * #getAllPointerCaptureBehaviors(resizeHandlePosition: ResizeHandlePosition,
        resizeContainerBoundingClientRect: DOMRect): IterableIterator<PointerCaptureBehavior> {

        yield this.#resetRectPointerCaptureBehavior;

        const initializeArgs: ResizePointerCaptureBehaviorInitializeArgs = {
            resizeContainerBoundingClientRect,
            minimumWidth: this.context.minimumWidth,
            minimumHeight: this.context.minimumHeight
        };

        for (const behavior of this.#resizePointerCaptureBehaviors) {
            if (!behavior.appliesTo(resizeHandlePosition))
                continue;

            behavior.initialize(initializeArgs);

            yield behavior;
        }

        if (this.#additionalPointerCaptureBehaviors.size > 0)
            yield new AggregatePointerCaptureBehavior(this.#additionalPointerCaptureBehaviors);

        yield this.#setPositionPointerCaptureBehavior;
        yield this.#setSizePointerCaptureBehavior;
    }

    public dispose() {
        for (const resizeHandleInfo of this.context.resizeHandles)
            resizeHandleInfo.element.removeEventListener('pointerdown', this.#pointerDownEventListener);

        this.#additionalPointerCaptureBehaviors.clear();
    }

    public addPointerCaptureBehavior(pointerCaptureBehavior: PointerCaptureBehavior) {
        this.#additionalPointerCaptureBehaviors.add(pointerCaptureBehavior);
    }

    public removePointerCaptureBehavior(pointerCaptureBehavior: PointerCaptureBehavior) {
        this.#additionalPointerCaptureBehaviors.delete(pointerCaptureBehavior);
    }
}
