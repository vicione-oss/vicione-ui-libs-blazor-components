/**
 * Opt-in drag-end lifecycle detail. Composed into the drag ghost only when the drag ghost opts into it.
 */
export type DragEndListener = {
    /** Cancels in-flight work and releases per-drag state so the drag ghost is ready for the next drag. */
    dragEnd(): void;
};
