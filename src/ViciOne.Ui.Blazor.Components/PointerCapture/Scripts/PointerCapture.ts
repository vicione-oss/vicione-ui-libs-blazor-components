import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { PointerCaptureBehaviorContext } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior-context.js';
import { Point } from '/_content/ViciOne.Ui.Blazor.Components/js/point.js';

export class PointerCapture {
    public startedCssClass?: string;
    public endedCssClass?: string;
    public adjustForScrollValues = false;
    public behaviors?: PointerCaptureBehavior[];

    public onPointerMove?: (captureTarget: HTMLElement, x: number, y: number) => void;
    public onPointerUp?: (captureTarget: HTMLElement, x: number, y: number) => void;

    #userSelectBefore?: string;

    public start(pointerEvent: PointerEvent, captureTarget: HTMLElement, boundingClientRect: DOMRect) {
        let captureTargetPosition: Point;

        const pointerDownClientX = pointerEvent.clientX;
        const pointerDownClientY = pointerEvent.clientY;

        const captureTargetBoundingClientRect = captureTarget.getBoundingClientRect();

        const pointerDownCaptureTargetClientX = captureTargetBoundingClientRect.x;
        const pointerDownCaptureTargetClientY = captureTargetBoundingClientRect.y;

        const captureTargetClientXminimum = boundingClientRect.left;
        const captureTargetClientYminimum = boundingClientRect.top;

        const captureTargetClientXmaximum = captureTargetClientXminimum + boundingClientRect.width - captureTargetBoundingClientRect.width;
        const captureTargetClientYmaximum = captureTargetClientYminimum + boundingClientRect.height - captureTargetBoundingClientRect.height;

        const clearSelection = () => {
            window.getSelection()?.empty();
        };

        const avoidTextSelection = () => {
            this.#userSelectBefore = captureTarget.style.userSelect;

            captureTarget.style.userSelect = 'none';
        };

        const pointerMoveEventListener = (e: PointerEvent) => {
            const distanceX = e.clientX - pointerDownClientX;
            const distanceY = e.clientY - pointerDownClientY;

            let captureTargetClientX = pointerDownCaptureTargetClientX + distanceX;
            let captureTargetClientY = pointerDownCaptureTargetClientY + distanceY;

            if (captureTargetClientX < captureTargetClientXminimum)
                captureTargetClientX = captureTargetClientXminimum;

            if (captureTargetClientX > captureTargetClientXmaximum)
                captureTargetClientX = captureTargetClientXmaximum;

            if (captureTargetClientY < captureTargetClientYminimum)
                captureTargetClientY = captureTargetClientYminimum;

            if (captureTargetClientY > captureTargetClientYmaximum)
                captureTargetClientY = captureTargetClientYmaximum;

            let captureTargetX = captureTargetClientX - captureTargetClientXminimum;
            let captureTargetY = captureTargetClientY - captureTargetClientYminimum;

            if (this.adjustForScrollValues) {
                captureTargetX += window.scrollX;
                captureTargetY += window.scrollY;
            }

            captureTargetPosition = new Point(captureTargetX, captureTargetY);

            if (this.behaviors?.length) {
                const behaviorContext = new PointerCaptureBehaviorContext(captureTarget, captureTargetPosition);

                applyBehaviors(this.behaviors.values(), behaviorContext);
            }

            captureTarget.style.left = `${captureTargetPosition.x}px`;
            captureTarget.style.top = `${captureTargetPosition.y}px`;

            if (this.onPointerMove)
                this.onPointerMove(captureTarget, captureTargetPosition.x, captureTargetPosition.y);
        };

        const applyBehaviors = (behaviors: IterableIterator<PointerCaptureBehavior>,
            behaviorContext: PointerCaptureBehaviorContext) => {

            const currentBehaviorIteratorResult = behaviors.next();
            if (currentBehaviorIteratorResult.done)
                return;

            const currentBehavior = currentBehaviorIteratorResult.value;

            const next = (behaviorContext: PointerCaptureBehaviorContext) => {
                const nextBehaviorIteratorResult = behaviors.next();
                if (nextBehaviorIteratorResult.done)
                    return;

                const nextBehavior = nextBehaviorIteratorResult.value;

                nextBehavior.apply(behaviorContext, next);
            };

            currentBehavior.apply(behaviorContext, next);
        };

        const pointerUpEventListener = async (e: PointerEvent) => {
            captureTarget.removeEventListener('pointerup', pointerUpEventListener);
            captureTarget.removeEventListener('pointermove', pointerMoveEventListener);
            captureTarget.releasePointerCapture(e.pointerId);

            if (this.startedCssClass)
                captureTarget.classList.remove(this.startedCssClass);

            if (this.endedCssClass)
                captureTarget.classList.add(this.endedCssClass);

            if (this.#userSelectBefore)
                captureTarget.style.userSelect = this.#userSelectBefore;

            // Sometimes pointer up is selecting text, we revert it
            clearSelection();

            if (this.onPointerUp) {

                // Handle missing pointerMove event, this can happen when the user clicks but does not move the mouse
                if (!captureTargetPosition) {
                    let captureTargetX = captureTarget.offsetLeft;
                    let captureTargetY = captureTarget.offsetTop;

                    if (this.adjustForScrollValues) {
                        captureTargetX += window.scrollX;
                        captureTargetY += window.scrollY;
                    }

                    captureTargetPosition = new Point(captureTargetX, captureTargetY);

                    if (this.behaviors?.length) {
                        const behaviorContext = new PointerCaptureBehaviorContext(captureTarget, captureTargetPosition);

                        applyBehaviors(this.behaviors.values(), behaviorContext);
                    }
                }

                this.onPointerUp(captureTarget, captureTargetPosition.x, captureTargetPosition.y);
            }
        };

        // User could have selected text before start dragging, we clear the selection to avoid confusion ...
        clearSelection();

        // ... and we avoid that new text can be selected before start of the pointer capture
        avoidTextSelection();

        if (this.endedCssClass)
            captureTarget.classList.remove(this.endedCssClass);

        if (this.startedCssClass)
            captureTarget.classList.add(this.startedCssClass);

        captureTarget.setPointerCapture(pointerEvent.pointerId);
        captureTarget.addEventListener('pointermove', pointerMoveEventListener);
        captureTarget.addEventListener('pointerup', pointerUpEventListener);
    }
}
