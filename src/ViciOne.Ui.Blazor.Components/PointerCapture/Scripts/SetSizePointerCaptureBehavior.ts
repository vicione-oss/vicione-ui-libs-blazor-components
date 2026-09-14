import { type PointerCaptureBehavior } from './PointerCaptureBehavior.ts';
import { type PointerCaptureBehaviorContext } from './PointerCaptureBehaviorContext.ts';

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
