import { type PointerCaptureBehavior } from '../../PointerCapture/Scripts/PointerCaptureBehavior.ts';
import { type ResizeHandlePosition } from '../Enums/ResizeHandlePosition.cs.ts';
import { type ResizePointerCaptureBehaviorInitializeArgs } from './ResizePointerCaptureBehaviorInitializeArgs.ts';

export type ResizePointerCaptureBehavior = PointerCaptureBehavior & {
    appliesTo(resizeHandlePosition: ResizeHandlePosition): boolean;
    initialize(args: ResizePointerCaptureBehaviorInitializeArgs): void;
};
