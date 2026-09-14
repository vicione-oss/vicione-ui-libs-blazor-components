import { type DragGhostContentSource } from './DragGhostContentSource.ts';
import { type DragGhostDraggableLink } from './DragGhostDraggableLink.ts';
import { type DragGhostContentChangedNotifier } from './DragGhostContentChangedNotifier.ts';
import { type DragImminentListener } from './DragImminentListener.ts';
import { type DragStartListener } from './DragStartListener.ts';
import { type DragEndListener } from './DragEndListener.ts';
import { type DropzoneEnterListener } from './DropzoneEnterListener.ts';
import { type DropzoneLeaveListener } from './DropzoneLeaveListener.ts';

/**
 A drag ghost as resolved by the central `createDragGhost` factory and consumed by
 `DragInteraction`: the required content detail plus any opt-in details. Drag ghosts that build their clone from
 the dragged element implement `DragGhostDraggableLink` so `DragInteraction` can bind the draggable
 once when it attaches the drag ghost; drag ghosts that render from their own source (e.g. `DragGhostBase`)
 simply omit it. `DragInteraction` assigns `contentChanged` to learn when to re-fetch and swap the drag ghost
 content, and calls each opt-in lifecycle listener the drag ghost implements: `dragImminent` (synchronously
 on `pointerdown`, warming the content before the drag begins), `dragStart` (on the first move, when the drag
 actually begins), `dragEnd`, `dropzoneEnter` and `dropzoneLeave`.
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
