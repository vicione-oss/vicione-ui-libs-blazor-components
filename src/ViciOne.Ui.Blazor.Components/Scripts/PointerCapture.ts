export class PointerCapture {
    public startedCssClass?: string;
    public endedCssClass?: string;
    public adjustForScrollValues = false;

    public onPointerMove?: (captureTarget: HTMLElement, x: number, y: number) => void;
    public onPointerUp?: (captureTarget: HTMLElement, x: number, y: number) => void;

    private userSelectBefore?: string;

    public start(pointerEvent: PointerEvent, captureTarget: HTMLElement, boundingClientRect: DOMRect) {
        let captureTargetX: number;
        let captureTargetY: number;

        const pointerDownClientX = pointerEvent.clientX;
        const pointerDownClientY = pointerEvent.clientY;

        let { offsetX, offsetY } = pointerEvent;

        if (pointerEvent.currentTarget && pointerEvent.currentTarget !== pointerEvent.target) {
            // This path is entered when an element inside captureTarget raised the pointerEvent.
            // We need to adjust offsetX and offsetY as those values are relative to pointerEvent.target,
            // but we expect those values to be relative to pointerEvent.currentTarget (captureTarget)

            if (pointerEvent.currentTarget instanceof HTMLElement && pointerEvent.target instanceof HTMLElement) {
                const currentTargetBoundingClientRect = pointerEvent.currentTarget.getBoundingClientRect();
                const targetBoundingClientRect = pointerEvent.target.getBoundingClientRect();

                offsetX += targetBoundingClientRect.left - currentTargetBoundingClientRect.left;
                offsetY += targetBoundingClientRect.top - currentTargetBoundingClientRect.top;
            }
        }

        const pointerDownCaptureTargetClientX = pointerEvent.clientX - offsetX;
        const pointerDownCaptureTargetClientY = pointerEvent.clientY - offsetY;

        const captureTargetClientXminimum = boundingClientRect.left;
        const captureTargetClientYminimum = boundingClientRect.top;

        const captureTargetBoundingClientRect = captureTarget.getBoundingClientRect();
        const captureTargetClientXmaximum = captureTargetClientXminimum + boundingClientRect.width - captureTargetBoundingClientRect.width;
        const captureTargetClientYmaximum = captureTargetClientYminimum + boundingClientRect.height - captureTargetBoundingClientRect.height;

        const clearSelection = () => {
            window.getSelection()?.empty();
        };

        const avoidTextSelection = () => {
            this.userSelectBefore = captureTarget.style.userSelect;

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

            captureTargetX = captureTargetClientX - captureTargetClientXminimum;
            captureTargetY = captureTargetClientY - captureTargetClientYminimum;

            if (this.adjustForScrollValues) {
                captureTargetX += window.scrollX;
                captureTargetY += window.scrollY;
            }

            captureTarget.style.left = `${captureTargetX}px`;
            captureTarget.style.top = `${captureTargetY}px`;

            if (this.onPointerMove)
                this.onPointerMove(captureTarget, captureTargetX, captureTargetY);
        };

        const pointerUpEventListener = async (e: PointerEvent) => {
            captureTarget.removeEventListener('pointerup', pointerUpEventListener);
            captureTarget.removeEventListener('pointermove', pointerMoveEventListener);
            captureTarget.releasePointerCapture(e.pointerId);

            if (this.startedCssClass)
                captureTarget.classList.remove(this.startedCssClass);

            if (this.endedCssClass)
                captureTarget.classList.add(this.endedCssClass);

            if (this.userSelectBefore)
                captureTarget.style.userSelect = this.userSelectBefore;

            // Sometimes pointer up is selecting text, we revert it
            clearSelection();

            if (this.onPointerUp) {
                // Handle missing pointerMove event, this can happen when the user clicks but does not move the mouse
                if (!captureTargetX) {
                    captureTargetX = captureTarget.offsetLeft;

                    if (this.adjustForScrollValues)
                        captureTargetX += window.scrollX;
                }

                if (!captureTargetY) {
                    captureTargetY = captureTarget.offsetTop;

                    if (this.adjustForScrollValues)
                        captureTargetY += window.scrollY;
                }

                this.onPointerUp(captureTarget, captureTargetX, captureTargetY);
            }
        };

        // User could have selected text before start dragging, we clear the selection to avoid confusion ...
        clearSelection();

        // ... and we avoid that new text can be selected before start of the pointer captuure
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
