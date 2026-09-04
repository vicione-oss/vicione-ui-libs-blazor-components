import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { type PointerCaptureBehaviorContext } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior-context.js';

export class SetSizePointerCaptureBehavior implements PointerCaptureBehavior {

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        const { element, rect, originalRect } = context.captureTarget;

        if (rect.width !== originalRect.width)
            element.style.width = `${rect.width}px`;

        if (rect.height !== originalRect.height)
            element.style.height = `${rect.height}px`;

        next(context);
    }
}
