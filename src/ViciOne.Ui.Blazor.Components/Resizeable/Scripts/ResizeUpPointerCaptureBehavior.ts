import { type PointerCaptureBehaviorContext } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior-context.js';
import { type ResizePointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/scripts/resize-pointer-capture-behavior.js';
import { type ResizePointerCaptureBehaviorInitializeArgs } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/scripts/resize-pointer-capture-behavior-initialize-args.js';
import { ResizeHandlePosition } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/enums/resize-handle-position.js';
import '/_content/ViciOne.Ui.Blazor.Components/js/number-mixins.js';

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
