import { type PointerCaptureBehaviorContext } from './PointerCaptureBehaviorContext.ts';

export type PointerCaptureBehavior = {
    apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void): void;
};
