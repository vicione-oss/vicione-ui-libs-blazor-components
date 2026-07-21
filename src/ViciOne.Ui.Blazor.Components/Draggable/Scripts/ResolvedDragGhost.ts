import { type DragGhostContentSource } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-ghost-content-source.js';
import { type DragGhostDraggableLink } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-ghost-draggable-link.js';
import { type DragGhostContentChangedNotifier } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-ghost-content-changed-notifier.js';
import { type DragImminentListener } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-imminent-listener.js';
import { type DragStartListener } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-start-listener.js';
import { type DragEndListener } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-end-listener.js';
import { type DropzoneEnterListener } from '/_content/ViciOne.Ui.Blazor.Components/draggable/dropzone-enter-listener.js';
import { type DropzoneLeaveListener } from '/_content/ViciOne.Ui.Blazor.Components/draggable/dropzone-leave-listener.js';

/**
 * A drag ghost as resolved by the central `createDragGhost` factory and consumed by
 * `DragInteraction`: the required content detail plus any opt-in details. Drag ghosts that build their clone from
 * the dragged element implement `DragGhostDraggableLink` so `DragInteraction` can bind the draggable
 * once when it attaches the drag ghost; drag ghosts that render from their own source (e.g. `DragGhostBase`)
 * simply omit it. `DragInteraction` assigns `contentChanged` to learn when to re-fetch and swap the drag ghost
 * content, and calls each opt-in lifecycle listener the drag ghost implements: `dragImminent` (synchronously
 * on `pointerdown`, warming the content before the drag begins), `dragStart` (on the first move, when the drag
 * actually begins), `dragEnd`, `dropzoneEnter` and `dropzoneLeave`.
 */
export type ResolvedDragGhost =
    DragGhostContentSource &
    Partial<DragGhostDraggableLink> &
    Partial<DragGhostContentChangedNotifier> &
    Partial<DragImminentListener> &
    Partial<DragStartListener> &
    Partial<DragEndListener> &
    Partial<DropzoneEnterListener> &
    Partial<DropzoneLeaveListener>;
