/**
 Opt-in drag-imminent lifecycle detail. Composed into the drag ghost only when the drag ghost opts into it.
 */
export type DragImminentListener = {
    /**
     Prepares the content for the drag that is about to begin. `DragInteraction` calls this synchronously on
     `pointerdown`, while the pointer is held and **before** the drag has begun, so the drag ghost can build or
     refresh whatever {@link DragGhostContentSource.getContent} will return once the drag actually starts on the
     first pointer move. It warms the content during the press-before-move window: it must render nothing and
     must be synchronous, so no work here may be awaited. A `pointerdown` that never leads to a move (a plain
     click) warms the content and simply discards it on the next `dragImminent`.
     */
    dragImminent(): void;
};
