// Auto-generated code
import { type PointerCaptureBehavior } from '../../PointerCapture/Scripts/PointerCaptureBehavior.js';
import { type ModifierKey } from '../../Enums/ModifierKey.cs.js';
import { type DragGhostJsModuleDescriptor } from './DragGhostJsModuleDescriptor.cs.js';

export class DragInteractionContext {
    // eslint-disable-next-line max-params
    constructor(readonly draggableId: string,
        readonly draggable: HTMLElement,
        readonly startedCssClass: string,
        readonly ongoingCssClass: string,
        readonly endedCssClass: string,

        // eslint-disable-next-line @typescript-eslint/no-restricted-types
        readonly modifierKey: ModifierKey | null,

        readonly dotNetObject: DotNet.DotNetObject,

        // eslint-disable-next-line @typescript-eslint/no-restricted-types
        readonly pointerCaptureBehaviors: PointerCaptureBehavior[] | null,

        // eslint-disable-next-line @typescript-eslint/no-restricted-types
        readonly dragGhostJsModule: DragGhostJsModuleDescriptor | null) {}
}
