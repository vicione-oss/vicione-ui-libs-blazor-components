// Auto-generated code
import { type BlazorNullable } from '../../Scripts/BlazorNullable.ts';
import { type PointerCaptureBehavior } from '../../PointerCapture/Scripts/PointerCaptureBehavior.ts';

export class MoveInteractionContext {
    // eslint-disable-next-line max-params
    constructor(readonly moveableId: string,
        readonly moveable: HTMLElement,
        readonly moveHandle: HTMLElement,
        readonly moveContainer: HTMLElement,
        readonly startedCssClass: string,
        readonly ongoingCssClass: string,
        readonly endedCssClass: string,
        readonly dotNetObject: DotNet.DotNetObject,
        readonly pointerCaptureBehaviors: BlazorNullable<PointerCaptureBehavior[]>) {}
}
