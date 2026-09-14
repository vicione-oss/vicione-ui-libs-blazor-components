import '../../Scripts/NumberMixins.ts';
import { type PointerCaptureBehavior } from './PointerCaptureBehavior.ts';
import { type PointerCaptureBehaviorContext } from './PointerCaptureBehaviorContext.ts';

// Re-baselines the capture origin and size mid-gesture without mutating the intentionally readonly
// originalRect. When a clone element changes size mid-drag, the consumer records the grab-fraction shift
// and the new size here; on the next pipeline run this behavior recomputes the writeable rect from the
// readonly originalRect plus the accumulated shift, keeping the grabbed fraction under the pointer and
// clamping against the new size. Runs after MovePointerCaptureBehavior, whose output it overrides while a
// re-anchor is active.
export class ReanchorPointerCaptureBehavior implements PointerCaptureBehavior {

    #clampBoundingClientRect = new DOMRect();
    #shiftX = 0;
    #shiftY = 0;
    #width?: number;
    #height?: number;

    public initialize(clampBoundingClientRect: DOMRect) {
        this.#clampBoundingClientRect = clampBoundingClientRect;
        this.#shiftX = 0;
        this.#shiftY = 0;
        this.#width = undefined;
        this.#height = undefined;
    }

    // Records a size change of the captured element. The shift keeps the grabbed fraction under the pointer;
    // it accumulates across successive swaps, while the size always reflects the latest element size.
    public reanchor(shiftX: number, shiftY: number, width: number, height: number) {
        this.#shiftX += shiftX;
        this.#shiftY += shiftY;
        this.#width = width;
        this.#height = height;
    }

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        if (this.#width === undefined || this.#height === undefined) {
            next(context);

            return;
        }

        const { rect, originalRect } = context.captureTarget;

        const maximumLeft = this.#clampBoundingClientRect.width - this.#width;
        const maximumTop = this.#clampBoundingClientRect.height - this.#height;

        const newLeft = (originalRect.left + this.#shiftX + context.distanceX).clamp(0, maximumLeft);
        const newTop = (originalRect.top + this.#shiftY + context.distanceY).clamp(0, maximumTop);

        rect.left = newLeft;
        rect.top = newTop;
        rect.right = newLeft + this.#width;
        rect.bottom = newTop + this.#height;

        next(context);
    }
}
