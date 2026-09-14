import { type CaptureTarget } from './CaptureTarget.ts';

/**
 The readonly view of the behavior context handed to behaviors.
 */
export type PointerCaptureBehaviorContext = {
    /**
    The capture target with its positional and dimensional state.
    */
    readonly captureTarget: CaptureTarget;

    /**
     The horizontal distance the pointer has moved from the initial pointer-down position.

     Behaviors treat the horizontaldistance as a readonly source for their computations;
     only the owning pointer capture updates it through {@link MutablePointerCaptureBehaviorContext}.
     */
    readonly distanceX: number;

    /**
     The vertical distance the pointer has moved from the initial pointer-down position.

     Behaviors treat the vertical distance as a readonly source for their computations;
     only the owning pointer capture updates it through {@link MutablePointerCaptureBehaviorContext}.
     */
    readonly distanceY: number;
};
