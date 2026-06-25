import { type PointerCaptureBehaviorContext } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior-context.js';
import { type ResizePointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/scripts/resize-pointer-capture-behavior.js';
import { type ResizePointerCaptureBehaviorInitializeArgs } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/scripts/resize-pointer-capture-behavior-initialize-args.js';
import { ResizeHandlePosition } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/enums/resize-handle-position.js';
import '/_content/ViciOne.Ui.Blazor.Components/js/number-mixins.js';

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
