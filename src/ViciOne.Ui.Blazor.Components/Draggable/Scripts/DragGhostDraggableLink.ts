/**
 Opt-in detail that links a drag ghost to its draggable for the lifetime of the drag ghost.

 Drag ghosts that build their clone from the dragged element implement this detail; drag ghosts that render from
 their own source (e.g. `DragGhostBase`) omit it. When present, `DragInteraction` calls `setDraggable`
 once when it attaches the drag ghost, before any other detail is used, and `clearDraggable` when the drag
 ends so the drag ghost can drop its reference to the draggable (important once drag ghost instances are pooled and
 reused across drags). The drag ghost keeps the reference and reads from it in `getContent` and the lifecycle
 callbacks, so the draggable no longer needs to be threaded through every method.
 */
export type DragGhostDraggableLink = {
    setDraggable(draggable: HTMLElement): void;
    clearDraggable(): void;
};
