import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { type PointerCaptureBehaviorContext } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior-context.js';

export class SetSizePointerCaptureBehavior implements PointerCaptureBehavior {

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        context.captureTarget.element.style.width = `${context.captureTarget.rect.width}px`;
        context.captureTarget.element.style.height = `${context.captureTarget.rect.height}px`;

        next(context);
    }
}
