import { type PointerCaptureBehaviorContext } from '../../PointerCapture/Scripts/PointerCaptureBehaviorContext.ts';
import '../../Scripts/NumberMixins.ts';
import { ResizeHandlePosition } from '../Enums/ResizeHandlePosition.cs.ts';
import { type ResizePointerCaptureBehavior } from './ResizePointerCaptureBehavior.ts';
import { type ResizePointerCaptureBehaviorInitializeArgs } from './ResizePointerCaptureBehaviorInitializeArgs.ts';

export class ResizeDownPointerCaptureBehavior implements ResizePointerCaptureBehavior {

    #resizeContainerHeight = 0;
    #minimumHeight = 0;

    public appliesTo(resizeHandlePosition: ResizeHandlePosition): boolean {
        return [ResizeHandlePosition.BottomRight, ResizeHandlePosition.Bottom, ResizeHandlePosition.BottomLeft]
            .includes(resizeHandlePosition);
    }

    public initialize(args: ResizePointerCaptureBehaviorInitializeArgs) {
        this.#resizeContainerHeight = args.resizeContainerBoundingClientRect.height;
        this.#minimumHeight = args.minimumHeight;
    }

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        const { rect, originalRect } = context.captureTarget;

        const minBottom = rect.top + this.#minimumHeight;

        rect.bottom = (originalRect.bottom + context.distanceY).clamp(minBottom, this.#resizeContainerHeight);

        next(context);
    }
}
