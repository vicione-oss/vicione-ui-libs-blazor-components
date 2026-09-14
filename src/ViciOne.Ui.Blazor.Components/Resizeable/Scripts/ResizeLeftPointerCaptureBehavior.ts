import { type PointerCaptureBehaviorContext } from '../../PointerCapture/Scripts/PointerCaptureBehaviorContext.ts';
import '../../Scripts/NumberMixins.ts';
import { ResizeHandlePosition } from '../Enums/ResizeHandlePosition.cs.ts';
import { type ResizePointerCaptureBehavior } from './ResizePointerCaptureBehavior.ts';
import { type ResizePointerCaptureBehaviorInitializeArgs } from './ResizePointerCaptureBehaviorInitializeArgs.ts';

export class ResizeLeftPointerCaptureBehavior implements ResizePointerCaptureBehavior {

    #minimumWidth = 0;

    public appliesTo(resizeHandlePosition: ResizeHandlePosition): boolean {
        return [ResizeHandlePosition.BottomLeft, ResizeHandlePosition.Left, ResizeHandlePosition.TopLeft].includes(resizeHandlePosition);
    }

    public initialize(args: ResizePointerCaptureBehaviorInitializeArgs) {
        this.#minimumWidth = args.minimumWidth;
    }

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        const { rect, originalRect } = context.captureTarget;

        const maxLeft = rect.right - this.#minimumWidth;

        rect.left = (originalRect.left + context.distanceX).clamp(0, maxLeft);

        next(context);
    }
}
