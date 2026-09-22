// Auto-generated code
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

        // eslint-disable-next-line @typescript-eslint/no-restricted-types
        readonly pointerCaptureBehaviors: PointerCaptureBehavior[] | null) {}
}
