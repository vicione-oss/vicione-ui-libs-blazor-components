// Auto-generated code
import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { type ModifierKey } from '/_content/ViciOne.Ui.Blazor.Components/enums/modifier-key.js';
import { type DragGhostJsModuleDescriptor } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-ghost-js-module-descriptor.js';

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
