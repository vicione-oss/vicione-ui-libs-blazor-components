import { type BlazorNullable } from '../../../Scripts/BlazorNullable.ts';
import { type PointerCaptureBehavior } from '../../Scripts/PointerCaptureBehavior.ts';
import { type PointerCaptureBehaviorContext } from '../../Scripts/PointerCaptureBehaviorContext.ts';
import { type CaptureTargetRect } from '../../Scripts/CaptureTargetRect.ts';

class SnapToGridPointerCaptureBehavior implements PointerCaptureBehavior {
    #gridSize: number;

    constructor(gridSize: number) {
        this.#gridSize = gridSize;
    }

    #snap(rect: CaptureTargetRect, lastRect: Readonly<CaptureTargetRect>, edge: 'left' | 'top' | 'right' | 'bottom'): boolean {
        const value = rect[edge];
        const lastValue = lastRect[edge];

        if (value === lastValue)
            return false;

        const remainder = value % this.#gridSize;
        if (remainder === 0)
            return false;

        rect[edge] -= remainder;

        return true;
    }

    public setGridSize(value: number) {

        this.#gridSize = value;
    }

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        const { rect, lastRect } = context.captureTarget;

        const snappedLeft = this.#snap(rect, lastRect, 'left');
        const snappedTop = this.#snap(rect, lastRect, 'top');
        const snappedRight = this.#snap(rect, lastRect, 'right');
        const snappedBottom = this.#snap(rect, lastRect, 'bottom');

        if (!snappedLeft && !snappedTop && !snappedRight && !snappedBottom)
            next(context);
    }
}

export async function createInstance(gridSize: BlazorNullable<number>) {
    return new SnapToGridPointerCaptureBehavior(gridSize ?? 10);
}
