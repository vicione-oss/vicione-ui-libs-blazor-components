import { type PointerCaptureBehaviorContext } from '../../PointerCapture/Scripts/PointerCaptureBehaviorContext.ts';
import '../../Scripts/NumberMixins.ts';
import { ResizeHandlePosition } from '../Enums/ResizeHandlePosition.cs.ts';
import { type ResizePointerCaptureBehavior } from './ResizePointerCaptureBehavior.ts';
import { type ResizePointerCaptureBehaviorInitializeArgs } from './ResizePointerCaptureBehaviorInitializeArgs.ts';

export class ResizeRightPointerCaptureBehavior implements ResizePointerCaptureBehavior {

    #resizeContainerWidth = 0;
    #minimumWidth = 0;

    public appliesTo(resizeHandlePosition: ResizeHandlePosition): boolean {
        return [ResizeHandlePosition.TopRight, ResizeHandlePosition.Right, ResizeHandlePosition.BottomRight].includes(resizeHandlePosition);
    }

    public initialize(args: ResizePointerCaptureBehaviorInitializeArgs) {
        this.#resizeContainerWidth = args.resizeContainerBoundingClientRect.width;
        this.#minimumWidth = args.minimumWidth;
    }

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        const { rect, originalRect } = context.captureTarget;

        const minRight = rect.left + this.#minimumWidth;

        rect.right = (originalRect.right + context.distanceX).clamp(minRight, this.#resizeContainerWidth);

        next(context);
    }
}
