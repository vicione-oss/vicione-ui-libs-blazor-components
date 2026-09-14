import { type CaptureTarget } from './CaptureTarget.ts';
import { type PointerCaptureBehaviorContext } from './PointerCaptureBehaviorContext.ts';

/**
 The mutable behavior context owned by the pointer capture. It is passed to behaviors as the readonly
 {@link PointerCaptureBehaviorContext}, so only the pointer capture can update the pointer distance.
 */
export class MutablePointerCaptureBehaviorContext implements PointerCaptureBehaviorContext {
    /**
     Creates a new instance of {@link MutablePointerCaptureBehaviorContext}.
     @param captureTarget - The capture target with its positional and dimensional state.
     @param distanceX - The horizontal distance the pointer has moved from the initial pointer-down position.
     @param distanceY - The vertical distance the pointer has moved from the initial pointer-down position.
     */
    constructor(readonly captureTarget: CaptureTarget,
        public distanceX: number,
        public distanceY: number) {}
}
