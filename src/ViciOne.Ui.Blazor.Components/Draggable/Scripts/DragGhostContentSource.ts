/**
 * Required content detail of a drag ghost.
 */
export type DragGhostContentSource = {
    /**
     * Gets the content of the drag ghost.
     *
     * The drag interaction takes the returned element and places it inside its own drag ghost host. When the
     * element is a live, on-screen node (e.g. the draggable itself), it is cloned first so moving the drag ghost
     * never disturbs the original in the layout.
     */
    getContent(): HTMLElement;
};
