import { type PointerCaptureBehaviorContext } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior-context.js';

export type PointerCaptureBehavior = {
    apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void): void;
};
