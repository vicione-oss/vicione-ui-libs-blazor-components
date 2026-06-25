import { type PointerCaptureBehaviorContext } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior-context.js';
import { type ResizePointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/scripts/resize-pointer-capture-behavior.js';
import { type ResizePointerCaptureBehaviorInitializeArgs } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/scripts/resize-pointer-capture-behavior-initialize-args.js';
import { ResizeHandlePosition } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/enums/resize-handle-position.js';
import '/_content/ViciOne.Ui.Blazor.Components/js/number-mixins.js';

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
