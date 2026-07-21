/**
 * Lets a drag ghost notify `DragInteraction` that its content changed.
 *
 * `DragInteraction` assigns `contentChanged` when it attaches the drag ghost; the drag ghost raises it from its
 * lifecycle methods after producing new content. On each notification `DragInteraction` re-fetches the drag ghost
 * content via {@link DragGhostContentSource.getContent} and swaps it into the drag ghost. Any notification
 * raised after the drag ends is a guaranteed no-op.
 */
export type DragGhostContentChangedNotifier = {
    contentChanged?: () => void;
};
