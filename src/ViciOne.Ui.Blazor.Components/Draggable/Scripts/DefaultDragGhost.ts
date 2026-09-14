import { type DragGhostContentSource } from './DragGhostContentSource.ts';
import { type DragGhostDraggableLink } from './DragGhostDraggableLink.ts';

/**
 The default drag ghost used when no custom drag ghost module is configured or when resolving a
 configured module fails. It provides content only (no lifecycle callbacks): a detached clone of the live
 draggable, so the drag ghost can be appended to its host without moving the original out of its place.
 */
export class DefaultDragGhost implements DragGhostContentSource, DragGhostDraggableLink {
    #draggable: HTMLElement | undefined;

    public setDraggable(draggable: HTMLElement) {
        this.#draggable = draggable;
    }

    public clearDraggable() {
        this.#draggable = undefined;
    }

    public getContent(): HTMLElement {
        if (!this.#draggable)
            throw new Error('setDraggable must be called with the draggable before getContent.');

        return this.#draggable.cloneNode(true) as HTMLElement;
    }
}
