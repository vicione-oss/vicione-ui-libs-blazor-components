import { type PointerCaptureBehaviorContext } from '../../PointerCapture/Scripts/PointerCaptureBehaviorContext.ts';
import '../../Scripts/NumberMixins.ts';
import { ResizeHandlePosition } from '../Enums/ResizeHandlePosition.cs.ts';
import { type ResizePointerCaptureBehavior } from './ResizePointerCaptureBehavior.ts';
import { type ResizePointerCaptureBehaviorInitializeArgs } from './ResizePointerCaptureBehaviorInitializeArgs.ts';

export class ResizeUpPointerCaptureBehavior implements ResizePointerCaptureBehavior {

    #minimumHeight = 0;

    public appliesTo(resizeHandlePosition: ResizeHandlePosition): boolean {
        return [ResizeHandlePosition.TopLeft, ResizeHandlePosition.Top, ResizeHandlePosition.TopRight].includes(resizeHandlePosition);
    }

    public initialize(args: ResizePointerCaptureBehaviorInitializeArgs) {
        this.#minimumHeight = args.minimumHeight;
    }

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        const { rect, originalRect } = context.captureTarget;

        const maxTop = rect.bottom - this.#minimumHeight;

        rect.top = (originalRect.top + context.distanceY).clamp(0, maxTop);

        next(context);
    }
}
