import { PointerCapture } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture.js';
import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { type CaptureTarget } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/capture-target.js';
import { AggregatePointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/aggregate-pointer-capture-behavior.js';
import { MovePointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/move-pointer-capture-behavior.js';
import { AdjustForScrollPositionPointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/adjust-for-scroll-position-pointer-capture-behavior.js';
import { SetPositionPointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/set-position-pointer-capture-behavior.js';
import { ModifierKey } from '/_content/ViciOne.Ui.Blazor.Components/enums/modifier-key.js';
import { type DragInteractionContext } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-interaction-context.js';
import { type DropzoneDescriptor } from '/_content/ViciOne.Ui.Blazor.Components/draggable/dropzone-descriptor.js';
import '/_content/ViciOne.Ui.Blazor.Components/js/pointer-event-mixins.js';

class DragInteraction {
    readonly #movePointerCaptureBehavior = new MovePointerCaptureBehavior();
    readonly #adjustForScrollPositionPointerCaptureBehavior = new AdjustForScrollPositionPointerCaptureBehavior();
    readonly #setPositionPointerCaptureBehavior = new SetPositionPointerCaptureBehavior();
    readonly #additionalPointerCaptureBehaviors: Set<PointerCaptureBehavior> = new Set<PointerCaptureBehavior>();

    #dropzoneDescriptors: DropzoneDescriptor[] | undefined;
    #targetDropzoneDescriptor: DropzoneDescriptor | undefined;

    constructor(readonly context: DragInteractionContext) {
        context.pointerCaptureBehaviors?.forEach(b => this.#additionalPointerCaptureBehaviors.add(b));

        context.draggable.addEventListener('pointerdown', this.#pointerDownEventListener);
    }

    public dispose() {
        this.context.draggable.removeEventListener('pointerdown', this.#pointerDownEventListener);
    }

    public addPointerCaptureBehavior(pointerCaptureBehavior: PointerCaptureBehavior) {
        this.#additionalPointerCaptureBehaviors.add(pointerCaptureBehavior);
    }

    public removePointerCaptureBehavior(pointerCaptureBehavior: PointerCaptureBehavior) {
        this.#additionalPointerCaptureBehaviors.delete(pointerCaptureBehavior);
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
                pointerCapture.ongoingCssClass = this.context.ongoingCssClass;
                pointerCapture.endedCssClass = this.context.endedCssClass;
                pointerCapture.behaviors = [...this.#getAllPointerCaptureBehaviors(boundingClientRect)];
                pointerCapture.start(e, draggableClone, boundingClientRect);

                this.context.draggable.classList.add(this.context.startedCssClass);
            }
        }
    };

    readonly #draggableClonePointerMove = async (draggableClone: CaptureTarget) => {
        this.context.draggable.classList.remove(this.context.startedCssClass);
        this.context.draggable.classList.add(this.context.ongoingCssClass);

        const x = draggableClone.rect.left;
        const y = draggableClone.rect.top;

        const newTargetDropzoneDescriptor = this.#dropzoneDescriptors?.find(dropzoneDescriptor => {
            const dropzoneBoundingClientRect = dropzoneDescriptor.element.getBoundingClientRect();

            const dropzoneBoundingClientRectAdjusted = new DOMRect(dropzoneBoundingClientRect.x + window.scrollX,
                dropzoneBoundingClientRect.y + window.scrollY,
                dropzoneBoundingClientRect.width,
                dropzoneBoundingClientRect.height);

            return dropzoneBoundingClientRectAdjusted.x <= x && x < dropzoneBoundingClientRectAdjusted.right &&
                dropzoneBoundingClientRectAdjusted.y <= y && y < dropzoneBoundingClientRectAdjusted.bottom;
        });

        if (newTargetDropzoneDescriptor !== this.#targetDropzoneDescriptor) {
            if (this.#targetDropzoneDescriptor)
                await this.context.dotNetObject.invokeMethodAsync('DragLeaveAsync', this.#targetDropzoneDescriptor.id);

            this.#targetDropzoneDescriptor = newTargetDropzoneDescriptor;

            if (this.#targetDropzoneDescriptor)
                await this.context.dotNetObject.invokeMethodAsync('DragEnterAsync', this.context.draggableId, this.#targetDropzoneDescriptor.id);
        }
    };

    readonly #draggableClonePointerUp = async (draggableClone: CaptureTarget, pointerMoved: boolean) => {
        draggableClone.element.remove();

        this.context.draggable.classList.remove(this.context.startedCssClass);

        if (pointerMoved) {
            this.context.draggable.classList.remove(this.context.ongoingCssClass);
            this.context.draggable.classList.add(this.context.endedCssClass);

            let x = draggableClone.rect.left;
            let y = draggableClone.rect.top;

            await this.context.dotNetObject.invokeMethodAsync('DragEndAsync', this.context.draggableId, x, y);

            if (this.#targetDropzoneDescriptor) {
                const dropzoneBoundingClientRect = this.#targetDropzoneDescriptor.element.getBoundingClientRect();

                x -= dropzoneBoundingClientRect.x + window.scrollX;
                y -= dropzoneBoundingClientRect.y + window.scrollY;

                await this.context.dotNetObject.invokeMethodAsync('DragDroppedAsync', this.context.draggableId, this.#targetDropzoneDescriptor.id, x, y);
            }
        }
    };

    * #getAllPointerCaptureBehaviors(boundingClientRect: DOMRect): IterableIterator<PointerCaptureBehavior> {
        this.#movePointerCaptureBehavior.initialize(boundingClientRect);
        yield this.#movePointerCaptureBehavior;

        if (this.#additionalPointerCaptureBehaviors.size > 0)
            yield new AggregatePointerCaptureBehavior(this.#additionalPointerCaptureBehaviors);

        yield this.#adjustForScrollPositionPointerCaptureBehavior;
        yield this.#setPositionPointerCaptureBehavior;
    }
}

export async function attach(context: DragInteractionContext) {
    return new DragInteraction(context);
}
