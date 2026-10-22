// Auto-generated code
import { type BlazorNullable } from '../../Scripts/BlazorNullable.ts';
import { type PointerCaptureBehavior } from '../../PointerCapture/Scripts/PointerCaptureBehavior.ts';
import { type ModifierKey } from '../../Enums/ModifierKey.cs.ts';
import { type DragGhostJsModuleDescriptor } from './DragGhostJsModuleDescriptor.cs.ts';

export class DragInteractionContext {
    // eslint-disable-next-line max-params
    constructor(readonly draggableId: string,
        readonly draggable: HTMLElement,
        readonly startedCssClass: string,
        readonly ongoingCssClass: string,
        readonly endedCssClass: string,
        readonly modifierKey: BlazorNullable<ModifierKey>,
        readonly dotNetObject: DotNet.DotNetObject,
        readonly pointerCaptureBehaviors: BlazorNullable<PointerCaptureBehavior[]>,
        readonly dragGhostJsModule: BlazorNullable<DragGhostJsModuleDescriptor>) {}
}
