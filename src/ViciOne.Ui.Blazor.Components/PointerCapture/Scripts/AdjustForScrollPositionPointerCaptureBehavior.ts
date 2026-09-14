import { type PointerCaptureBehavior } from './PointerCaptureBehavior.ts';
import { type PointerCaptureBehaviorContext } from './PointerCaptureBehaviorContext.ts';

export class AdjustForScrollPositionPointerCaptureBehavior implements PointerCaptureBehavior {

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        if (context.captureTarget.element.parentElement === null)
            return;

        if (context.captureTarget.element.parentElement instanceof HTMLBodyElement) {
            context.captureTarget.rect.left += window.scrollX;
            context.captureTarget.rect.right += window.scrollX;
            context.captureTarget.rect.top += window.scrollY;
            context.captureTarget.rect.bottom += window.scrollY;
        } else {
            context.captureTarget.rect.left += context.captureTarget.element.parentElement.scrollLeft;
            context.captureTarget.rect.right += context.captureTarget.element.parentElement.scrollLeft;
            context.captureTarget.rect.top += context.captureTarget.element.parentElement.scrollTop;
            context.captureTarget.rect.bottom += context.captureTarget.element.parentElement.scrollTop;
        }

        next(context);
    }
}
