import { type CaptureTarget } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/capture-target.js';

export class PointerCaptureBehaviorContext {
    /**
     * Creates a new instance of {@link PointerCaptureBehaviorContext}.
     * @param captureTarget - The capture target with its positional and dimensional state.
     * @param distanceX - The horizontal distance the pointer has moved from the initial pointer-down position.
     * @param distanceY - The vertical distance the pointer has moved from the initial pointer-down position.
     */
    constructor(readonly captureTarget: CaptureTarget,
        public distanceX: number,
        public distanceY: number) { }
}
