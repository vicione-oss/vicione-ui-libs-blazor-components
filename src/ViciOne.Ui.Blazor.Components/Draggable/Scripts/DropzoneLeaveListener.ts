/**
 Opt-in dropzone-leave lifecycle detail. Composed into the drag ghost only when the drag ghost opts into it.
 */
export type DropzoneLeaveListener = {
    /**
     Reacts to the drag leaving a dropzone. When the reaction produces new content, the drag ghost raises its
     `contentChanged` notification so `DragInteraction` re-fetches and swaps the drag ghost content. Any notification
     raised after `DragEndListener.dragEnd` is a guaranteed no-op.
     */
    dropzoneLeave(): void;
};
