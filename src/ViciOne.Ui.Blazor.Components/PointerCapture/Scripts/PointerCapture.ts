import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { PointerCaptureBehaviorContext } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior-context.js';
import { PointerCaptureBehaviorPipeline } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior-pipeline.js';
import { CaptureTarget } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/capture-target.js';
import { CaptureTargetRect } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/capture-target-rect.js';

export class PointerCapture {
    public startedCssClass?: string;
    public ongoingCssClass?: string;
    public endedCssClass?: string;
    public behaviors?: PointerCaptureBehavior[];

    public onPointerMove?: (captureTarget: CaptureTarget) => void;
    public onPointerUp?: (captureTarget: CaptureTarget, pointerMoved: boolean) => void;

    #userSelectBefore?: string;
    #behaviorPipeline?: PointerCaptureBehaviorPipeline;

    public start(pointerEvent: PointerEvent, captureTargetElement: HTMLElement, boundingClientRect: DOMRect) {
        let pointerMoved = false;

        if (this.behaviors?.length)
            this.#behaviorPipeline = new PointerCaptureBehaviorPipeline(this.behaviors);

        const pointerDownClientX = pointerEvent.clientX;
        const pointerDownClientY = pointerEvent.clientY;

        const captureTargetBoundingClientRect = captureTargetElement.getBoundingClientRect();

        const clearSelection = () => {
            window.getSelection()?.empty();
        };

        const avoidTextSelection = () => {
            this.#userSelectBefore = captureTargetElement.style.userSelect;

            captureTargetElement.style.userSelect = 'none';
        };

        // User could have selected text before start dragging, we clear the selection to avoid confusion ...
        clearSelection();

        // ... and we avoid that new text can be selected before start of the pointer capture
        avoidTextSelection();

        if (this.endedCssClass)
            captureTargetElement.classList.remove(this.endedCssClass);

        if (this.ongoingCssClass)
            captureTargetElement.classList.remove(this.ongoingCssClass);

        if (this.startedCssClass)
            captureTargetElement.classList.add(this.startedCssClass);

        const originalX = captureTargetBoundingClientRect.x - boundingClientRect.left;
        const originalY = captureTargetBoundingClientRect.y - boundingClientRect.top;

        const captureTargetRect = new CaptureTargetRect(
            originalX, originalY, originalX + captureTargetBoundingClientRect.width, originalY + captureTargetBoundingClientRect.height
        );

        const captureTarget = new CaptureTarget(captureTargetElement, captureTargetRect);

        const pointerMoveEventListener = (e: PointerEvent) => {
            pointerMoved = true;

            if (this.startedCssClass)
                captureTargetElement.classList.remove(this.startedCssClass);

            if (this.ongoingCssClass)
                captureTargetElement.classList.add(this.ongoingCssClass);

            const distanceX = e.clientX - pointerDownClientX;
            const distanceY = e.clientY - pointerDownClientY;

            if (this.#behaviorPipeline) {
                const behaviorContext = new PointerCaptureBehaviorContext(captureTarget, distanceX, distanceY);

                this.#behaviorPipeline.start(behaviorContext);
            }

            captureTarget.saveRect();

            if (this.onPointerMove)
                this.onPointerMove(captureTarget);
        };

        const pointerUpEventListener = async (e: PointerEvent) => {
            captureTargetElement.removeEventListener('pointerup', pointerUpEventListener);
            captureTargetElement.removeEventListener('pointermove', pointerMoveEventListener);
            captureTargetElement.releasePointerCapture(e.pointerId);

            if (this.startedCssClass)
                captureTargetElement.classList.remove(this.startedCssClass);

            if (this.ongoingCssClass)
                captureTargetElement.classList.remove(this.ongoingCssClass);

            if (this.endedCssClass && pointerMoved)
                captureTargetElement.classList.add(this.endedCssClass);

            captureTargetElement.style.userSelect = this.#userSelectBefore ?? '';

            // Sometimes pointer up is selecting text, we revert it
            clearSelection();

            if (this.onPointerUp)
                this.onPointerUp(captureTarget, pointerMoved);
        };

        captureTargetElement.setPointerCapture(pointerEvent.pointerId);
        captureTargetElement.addEventListener('pointermove', pointerMoveEventListener);
        captureTargetElement.addEventListener('pointerup', pointerUpEventListener);
    }
}
