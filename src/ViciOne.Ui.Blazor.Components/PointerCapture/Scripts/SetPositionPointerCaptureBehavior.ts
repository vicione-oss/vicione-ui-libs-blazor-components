import { type PointerCaptureBehavior } from './PointerCaptureBehavior.ts';
import { type PointerCaptureBehaviorContext } from './PointerCaptureBehaviorContext.ts';

export class SetPositionPointerCaptureBehavior implements PointerCaptureBehavior {

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        const { element, rect, originalRect } = context.captureTarget;

        if (rect.left !== originalRect.left)
            element.style.left = `${rect.left}px`;

        if (rect.top !== originalRect.top)
            element.style.top = `${rect.top}px`;

        next(context);
    }
}
