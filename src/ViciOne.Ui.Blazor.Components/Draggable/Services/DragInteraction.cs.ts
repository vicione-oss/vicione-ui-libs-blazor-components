import { PointerCapture } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture.js';
import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { ModifierKey } from '/_content/ViciOne.Ui.Blazor.Components/enums/modifier-key.js';
import { type DragInteractionContext } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-interaction-context.js';
import { type DropzoneDescriptor } from '/_content/ViciOne.Ui.Blazor.Components/draggable/dropzone-descriptor.js';
import '/_content/ViciOne.Ui.Blazor.Components/js/pointer-event-mixins.js';

class DragInteraction {
    readonly #pointerCaptureBehaviors: Set<PointerCaptureBehavior> = new Set<PointerCaptureBehavior>();

    #dropzoneDescriptors: DropzoneDescriptor[] | undefined;
    #targetDropzoneDescriptor: DropzoneDescriptor | undefined;

    constructor(readonly context: DragInteractionContext) {
        context.pointerCaptureBehaviors?.forEach(b => this.#pointerCaptureBehaviors.add(b));

        context.draggable.addEventListener('pointerdown', this.#pointerDownEventListener);
    }

    public dispose() {
        this.context.draggable.removeEventListener('pointerdown', this.#pointerDownEventListener);
    }

    public addPointerCaptureBehavior(pointerCaptureBehavior: PointerCaptureBehavior) {
        this.#pointerCaptureBehaviors.add(pointerCaptureBehavior);
    }

    public removePointerCaptureBehavior(pointerCaptureBehavior: PointerCaptureBehavior) {
        this.#pointerCaptureBehaviors.delete(pointerCaptureBehavior);
    }

    #hasRequiredKeyState(e: PointerEvent) {
        if (this.context.modifierKey === null)
            return true;

        const modifierKeyStr = ModifierKey[this.context.modifierKey];

        return e.getModifierState(modifierKeyStr);
    }

    readonly #pointerDownEventListener = async (e: PointerEvent) => {
        if (!this.#hasRequiredKeyState(e))
            return;

        if (e.isRaisedByElementWithOwnHandlerNestedIn(this.context.draggable, 'draggable'))
            return; // Nested draggable has its own handler

        if (!window.visualViewport) {
            console.error('Visual Viewport API not supported');

            return;
        }

        this.#dropzoneDescriptors = await this.context.dotNetObject.invokeMethodAsync('DragStartAsync', this.context.draggableId);
        if (this.#dropzoneDescriptors && this.#dropzoneDescriptors.length > 0) {

            const draggableClone = this.context.draggable.cloneNode(true);
            if (draggableClone instanceof HTMLElement) {

                const draggableBoundingClientRect = this.context.draggable.getBoundingClientRect();

                draggableClone.classList.add('draggable-clone');
                draggableClone.style.position = 'absolute';
                draggableClone.style.left = `${draggableBoundingClientRect.left + window.scrollX}px`;
                draggableClone.style.top = `${draggableBoundingClientRect.top + window.scrollY}px`;
                draggableClone.style.userSelect = 'none'; // Adopted from user-interaction.prevent-text-selection() > _user-interaction.scss
                draggableClone.style.opacity = '0.3';

                document.body.append(draggableClone);

                const boundingClientRect = new DOMRect(0, 0, window.visualViewport.width, window.visualViewport.height);

                const pointerCapture = new PointerCapture();
                pointerCapture.onPointerMove = this.#draggableClonePointerMove;
                pointerCapture.onPointerUp = this.#draggableClonePointerUp;
                pointerCapture.startedCssClass = this.context.startedCssClass;
                pointerCapture.endedCssClass = this.context.endedCssClass;
                pointerCapture.adjustForScrollValues = true;
                pointerCapture.behaviors = this.#pointerCaptureBehaviors.size > 0 ? [...this.#pointerCaptureBehaviors] : undefined;
                pointerCapture.start(e, draggableClone, boundingClientRect);

                this.context.draggable.classList.add(this.context.startedCssClass);
            }
        }
    };

    readonly #draggableClonePointerMove = async (_draggableClone: HTMLElement, x: number, y: number) => {
        const newTargetDropzoneDescriptor = this.#dropzoneDescriptors?.find(dropzoneDescriptor => {
            const dropzoneBoundingClientRect = dropzoneDescriptor.element.getBoundingClientRect();

            const dropzoneBoundingClientRectAdjusted = new DOMRect(dropzoneBoundingClientRect.x + window.scrollX,
                dropzoneBoundingClientRect.y + window.scrollY,
                dropzoneBoundingClientRect.width,
                dropzoneBoundingClientRect.height);

            return dropzoneBoundingClientRectAdjusted.x <= x && x < dropzoneBoundingClientRectAdjusted.right
                && dropzoneBoundingClientRectAdjusted.y <= y && y < dropzoneBoundingClientRectAdjusted.bottom;
        });

        if (newTargetDropzoneDescriptor !== this.#targetDropzoneDescriptor) {
            if (this.#targetDropzoneDescriptor)
                await this.context.dotNetObject.invokeMethodAsync('DragLeaveAsync', this.#targetDropzoneDescriptor.id);

            this.#targetDropzoneDescriptor = newTargetDropzoneDescriptor;

            if (this.#targetDropzoneDescriptor)
                await this.context.dotNetObject.invokeMethodAsync('DragEnterAsync', this.context.draggableId, this.#targetDropzoneDescriptor.id);
        }
    };

    readonly #draggableClonePointerUp = async (draggableClone: HTMLElement, x: number, y: number) => {
        draggableClone.remove();

        this.context.draggable.classList.remove(this.context.startedCssClass);
        this.context.draggable.classList.add(this.context.endedCssClass);

        await this.context.dotNetObject.invokeMethodAsync('DragEndAsync', this.context.draggableId, x, y);

        if (this.#targetDropzoneDescriptor) {
            const dropzoneBoundingClientRect = this.#targetDropzoneDescriptor.element.getBoundingClientRect();

            x -= dropzoneBoundingClientRect.x + window.scrollX;
            y -= dropzoneBoundingClientRect.y + window.scrollY;

            await this.context.dotNetObject.invokeMethodAsync('DragDroppedAsync', this.context.draggableId, this.#targetDropzoneDescriptor.id, x, y);
        }
    };
}

export async function attach(context: DragInteractionContext) {
    return new DragInteraction(context);
}
