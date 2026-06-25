import { PointerCapture } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture.js';
import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { type ResizeInteractionContext } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/resize-interaction-context.js';
import { AggregatePointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/aggregate-pointer-capture-behavior.js';
import { ResetRectPointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/reset-rect-pointer-capture-behavior.js';
import { ResizeLeftPointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/scripts/resize-left-pointer-capture-behavior.js';
import { ResizeRightPointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/scripts/resize-right-pointer-capture-behavior.js';
import { ResizeUpPointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/scripts/resize-up-pointer-capture-behavior.js';
import { ResizeDownPointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/scripts/resize-down-pointer-capture-behavior.js';
import { SetPositionPointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/set-position-pointer-capture-behavior.js';
import { SetSizePointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/set-size-pointer-capture-behavior.js';
import { type ResizePointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/scripts/resize-pointer-capture-behavior.js';
import { type ResizePointerCaptureBehaviorInitializeArgs } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/scripts/resize-pointer-capture-behavior-initialize-args.js';
import { type ResizeHandlePosition } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/enums/resize-handle-position.js';
import '/_content/ViciOne.Ui.Blazor.Components/js/pointer-event-mixins.js';

class ResizeInteraction {
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

    constructor(readonly context: ResizeInteractionContext) {
        this.context.pointerCaptureBehaviors?.forEach(b => this.#additionalPointerCaptureBehaviors.add(b));

        this.context.resizeHandles.forEach(resizeHandleInfo => {
            resizeHandleInfo.element.addEventListener('pointerdown', this.#pointerDownEventListener);
        });
    }

    public dispose() {
        this.context.resizeHandles.forEach(resizeHandleInfo => {
            resizeHandleInfo.element.removeEventListener('pointerdown', this.#pointerDownEventListener);
        });

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

        const resizeHandleInfo = this.context.resizeHandles.find(h => h.element === e.currentTarget);

        if (resizeHandleInfo === undefined)
            return;

        const resizeContainerBoundingClientRect = this.context.resizeContainer.getBoundingClientRect();

        const pointerCapture = new PointerCapture();
        pointerCapture.startedCssClass = this.context.startedCssClass;
        pointerCapture.ongoingCssClass = this.context.ongoingCssClass;
        pointerCapture.endedCssClass = this.context.endedCssClass;
        pointerCapture.behaviors = [...this.#getAllPointerCaptureBehaviors(resizeHandleInfo.position, resizeContainerBoundingClientRect)];

        pointerCapture.onPointerUp = async (captureTarget, pointerMoved) => {
            if (pointerMoved) {
                const domRect = captureTarget.rect.toDomRect();

                await this.context.dotNetObject.invokeMethodAsync('OnUpdatePositionAndSizeAsync', this.context.resizeableId, domRect);
            }
        };

        pointerCapture.start(e, this.context.resizeable, resizeContainerBoundingClientRect);
    };

    * #getAllPointerCaptureBehaviors(resizeHandlePosition: ResizeHandlePosition, resizeContainerBoundingClientRect: DOMRect): IterableIterator<PointerCaptureBehavior> {
        yield this.#resetRectPointerCaptureBehavior;

        const initializeArgs: ResizePointerCaptureBehaviorInitializeArgs = {
            resizeContainerBoundingClientRect,
            minimumWidth: this.context.minimumWidth,
            minimumHeight: this.context.minimumHeight
        };

        for (const behavior of this.#resizePointerCaptureBehaviors) {
            if (behavior.appliesTo(resizeHandlePosition)) {
                behavior.initialize(initializeArgs);

                yield behavior;
            }
        }

        if (this.#additionalPointerCaptureBehaviors.size > 0)
            yield new AggregatePointerCaptureBehavior(this.#additionalPointerCaptureBehaviors);

        yield this.#setPositionPointerCaptureBehavior;
        yield this.#setSizePointerCaptureBehavior;
    }
}

export async function attach(context: ResizeInteractionContext) {
    const resizeInteraction = new ResizeInteraction(context);

    return resizeInteraction;
}
