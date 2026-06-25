import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { type ResizeHandlePosition } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/enums/resize-handle-position.js';
import { type ResizePointerCaptureBehaviorInitializeArgs } from '/_content/ViciOne.Ui.Blazor.Components/resizeable/scripts/resize-pointer-capture-behavior-initialize-args.js';

export type ResizePointerCaptureBehavior = PointerCaptureBehavior & {
    appliesTo(resizeHandlePosition: ResizeHandlePosition): boolean;
    initialize(args: ResizePointerCaptureBehaviorInitializeArgs): void;
};
