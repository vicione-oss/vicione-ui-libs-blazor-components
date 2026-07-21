/**
 * Opt-in drag-start lifecycle detail. Composed into the drag ghost only when the drag ghost opts into it.
 */
export type DragStartListener = {
    /**
     * Kicks off any asynchronous work for the drag ghost. When the work produces new content, the drag ghost
     * raises its `contentChanged` notification so `DragInteraction` re-fetches and swaps the drag ghost content. Any
     * notification raised after `DragEndListener.dragEnd` is a guaranteed no-op.
     */
    dragStart(): void;
};
