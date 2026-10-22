// Auto-generated code
import { type BlazorNullable } from '../../Scripts/BlazorNullable.ts';
import { type PointerCaptureBehavior } from '../../PointerCapture/Scripts/PointerCaptureBehavior.ts';
import { type ResizeHandleInfo } from './ResizeHandleInfo.cs.ts';

export class ResizeInteractionContext {
    // eslint-disable-next-line max-params
    constructor(readonly resizeableId: string,
        readonly resizeable: HTMLElement,
        readonly resizeHandles: ResizeHandleInfo[],
        readonly resizeContainer: HTMLElement,
        readonly minimumWidth: number,
        readonly minimumHeight: number,
        readonly startedCssClass: string,
        readonly ongoingCssClass: string,
        readonly endedCssClass: string,
        readonly dotNetObject: DotNet.DotNetObject,
        readonly pointerCaptureBehaviors: BlazorNullable<PointerCaptureBehavior[]>) {}
}
