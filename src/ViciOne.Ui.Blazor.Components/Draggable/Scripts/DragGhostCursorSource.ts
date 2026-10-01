/**
 Opt-in cursor detail. Composed into the drag ghost only when the drag ghost opts into it.
 */
export type DragGhostCursorSource = {
    /**
     Gets the CSS cursor shown while the drag ghost is dragged, or `undefined` to keep the cursor the document
     shows. Read each time `DragInteraction` sets the drag ghost content.
     */
    getCursor(): string | undefined;
};
