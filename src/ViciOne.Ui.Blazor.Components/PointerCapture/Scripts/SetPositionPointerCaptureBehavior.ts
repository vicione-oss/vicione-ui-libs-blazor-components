import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { type PointerCaptureBehaviorContext } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior-context.js';

export class SetPositionPointerCaptureBehavior implements PointerCaptureBehavior {

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        context.captureTarget.element.style.left = `${context.captureTarget.rect.left}px`;
        context.captureTarget.element.style.top = `${context.captureTarget.rect.top}px`;

        next(context);
    }
}
