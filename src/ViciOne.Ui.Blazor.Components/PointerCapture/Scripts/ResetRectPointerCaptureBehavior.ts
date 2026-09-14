import { type PointerCaptureBehavior } from './PointerCaptureBehavior.ts';
import { type PointerCaptureBehaviorContext } from './PointerCaptureBehaviorContext.ts';

export class ResetRectPointerCaptureBehavior implements PointerCaptureBehavior {

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        const { rect, originalRect } = context.captureTarget;

        rect.left = originalRect.left;
        rect.top = originalRect.top;
        rect.right = originalRect.right;
        rect.bottom = originalRect.bottom;

        next(context);
    }
}
