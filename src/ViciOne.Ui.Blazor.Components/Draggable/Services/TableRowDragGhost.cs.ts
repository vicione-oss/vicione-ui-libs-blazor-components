import { type DragGhostContentSource } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-ghost-content-source.js';
import { type DragGhostDraggableLink } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-ghost-draggable-link.js';
import { type DragImminentListener } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-imminent-listener.js';
import { type DragEndListener } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-end-listener.js';

/**
 A drag ghost for table rows. Cloning a <tr> on its own drops the surrounding <table> that sized its
 columns, so the copied cells collapse to their content width. This drag ghost rebuilds the row as a self-contained
 table box and pins each cell to the width it had inside the real table, so the dragged row looks identical to
 the one being dragged. The clone is prepared up front on `dragImminent` and discarded on `dragEnd`.
 */
class TableRowDragGhost implements DragGhostContentSource, DragGhostDraggableLink, DragImminentListener, DragEndListener {
    readonly #emptyContent = document.createElement('div');

    #draggable: HTMLElement | undefined;
    #content: HTMLElement = this.#emptyContent;

    public setDraggable(draggable: HTMLElement) {
        this.#draggable = draggable;
    }

    public dragImminent() {
        if (!this.#draggable)
            return;

        // A cloned <tr> keeps its cells but loses the table context that sized them, so its columns collapse.
        // Make the drag ghost its own table box and pin each cell to the original cell's rendered width.

        const ghost = this.#draggable.cloneNode(true) as HTMLElement;

        ghost.style.display = 'table';
        ghost.style.tableLayout = 'fixed';

        // Assumes the row's direct children are the cells (<td>/<th>), pinned by index. Breaks if the
        // cells are wrapped (e.g. a component renders an element between <tr> and its cells), or if the
        // original and drag ghost cell order/count differ.
        const originalCells = this.#draggable.children;
        const ghostCells = ghost.children;
        for (let i = 0; i < originalCells.length && i < ghostCells.length; i++) {
            const cell = ghostCells[i];
            if (cell instanceof HTMLElement)
                cell.style.width = `${originalCells[i].getBoundingClientRect().width}px`;
        }

        this.#content = ghost;
    }

    public getContent(): HTMLElement {
        return this.#content;
    }

    public dragEnd() {
        this.#content = this.#emptyContent;
    }

    public clearDraggable() {
        this.#draggable = undefined;
    }
}

export function createDragGhost(): TableRowDragGhost {
    return new TableRowDragGhost();
}
