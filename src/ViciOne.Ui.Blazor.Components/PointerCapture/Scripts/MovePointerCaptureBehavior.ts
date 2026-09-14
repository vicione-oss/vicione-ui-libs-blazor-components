import '../../Scripts/NumberMixins.ts';
import { type PointerCaptureBehavior } from './PointerCaptureBehavior.ts';
import { type PointerCaptureBehaviorContext } from './PointerCaptureBehaviorContext.ts';

export class MovePointerCaptureBehavior implements PointerCaptureBehavior {

    #clampBoundingClientRect = new DOMRect();

    public initialize(clampBoundingClientRect: DOMRect) {
        this.#clampBoundingClientRect = clampBoundingClientRect;
    }

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        const { rect, originalRect } = context.captureTarget;

        const maximumLeft = this.#clampBoundingClientRect.width - originalRect.width;
        const maximumTop = this.#clampBoundingClientRect.height - originalRect.height;

        const newLeft = (originalRect.left + context.distanceX).clamp(0, maximumLeft);
        const newTop = (originalRect.top + context.distanceY).clamp(0, maximumTop);

        rect.left = newLeft;
        rect.top = newTop;
        rect.right = newLeft + originalRect.width;
        rect.bottom = newTop + originalRect.height;

        next(context);
    }
}
