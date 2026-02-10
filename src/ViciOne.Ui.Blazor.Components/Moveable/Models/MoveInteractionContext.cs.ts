// Auto-generated code
import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';

export class MoveInteractionContext {
    // eslint-disable-next-line max-params
    constructor(readonly moveableId: string,
        readonly moveable: HTMLElement,
        readonly moveHandle: HTMLElement,
        readonly moveContainer: HTMLElement,
        readonly startedCssClass: string,
        readonly endedCssClass: string,
        readonly dotNetObject: DotNet.DotNetObject,
        readonly pointerCaptureBehaviors: PointerCaptureBehavior[] | undefined) {}
}
