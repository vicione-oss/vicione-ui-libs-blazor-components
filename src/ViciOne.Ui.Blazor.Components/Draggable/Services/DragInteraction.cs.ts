import { PointerCapture } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture.js';
import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { type CaptureTarget } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/capture-target.js';
import { AggregatePointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/aggregate-pointer-capture-behavior.js';
import { MovePointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/move-pointer-capture-behavior.js';
import { ReanchorPointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/reanchor-pointer-capture-behavior.js';
import { AdjustForScrollPositionPointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/adjust-for-scroll-position-pointer-capture-behavior.js';
import { SetPositionPointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/set-position-pointer-capture-behavior.js';
import { ModifierKey } from '/_content/ViciOne.Ui.Blazor.Components/enums/modifier-key.js';
import { type DragInteractionContext } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-interaction-context.js';
import { type DropzoneDescriptor } from '/_content/ViciOne.Ui.Blazor.Components/draggable/dropzone-descriptor.js';
import { type DragGhostJsModuleDescriptor } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-ghost-js-module-descriptor.js';
import { type ResolvedDragGhost } from '/_content/ViciOne.Ui.Blazor.Components/draggable/resolved-drag-ghost.js';
import { DefaultDragGhost } from '/_content/ViciOne.Ui.Blazor.Components/draggable/default-drag-ghost.js';
import { DragGhostHost } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-ghost-host.js';
import '/_content/ViciOne.Ui.Blazor.Components/js/pointer-event-mixins.js';

class DragInteraction {
    readonly #movePointerCaptureBehavior = new MovePointerCaptureBehavior();
    readonly #reanchorPointerCaptureBehavior = new ReanchorPointerCaptureBehavior();
    readonly #adjustForScrollPositionPointerCaptureBehavior = new AdjustForScrollPositionPointerCaptureBehavior();
    readonly #setPositionPointerCaptureBehavior = new SetPositionPointerCaptureBehavior();
    readonly #additionalPointerCaptureBehaviors: Set<PointerCaptureBehavior> = new Set<PointerCaptureBehavior>();

    readonly #dragGhost: ResolvedDragGhost;

    #dropzoneDescriptors: DropzoneDescriptor[] | undefined;
    #targetDropzoneDescriptor: DropzoneDescriptor | undefined;

    // Per-drag state. One drag runs at a time. #grabFractionX/Y are captured on pointerdown; the rest
    // are (re)set when the drag begins on the first move and cleared on end.
    #dragTicket = 0;
    #dragGhostHost: DragGhostHost | undefined;
    #pointerCapture: PointerCapture | undefined;
    #grabFractionX = 0;
    #grabFractionY = 0;

    readonly #pointerDownEventListener = (event: PointerEvent) => {
        if (!this.#hasRequiredKeyState(event))
            return;

        if (event.isRaisedByElementWithOwnHandlerNestedIn(this.context.draggable, 'draggable'))
            return; // Nested draggable has its own handler

        // Record where inside the dragged element the pointer grabbed it, expressed as a fraction of the
        // element's size (0..1 on each axis): 0 = left/top edge, 0.5 = center, 1 = right/bottom edge. Example:
        // grabbing the horizontal middle of a 200px-wide element gives grabFractionX = 0.5. Storing it as a
        // fraction (not pixels) makes it size-independent, so the same relative point can be kept under the
        // pointer even when the drag ghost that follows the pointer has a different size than the dragged element
        // and even when the drag ghost is later swapped for a differently-sized one mid-drag. Measured here against
        // this fresh pointerdown event because the dragged element is the only thing on screen at grab time;
        // the drag ghost element does not exist yet.
        const draggableBoundingClientRect = this.context.draggable.getBoundingClientRect();
        this.#grabFractionX = (event.clientX - draggableBoundingClientRect.left) / draggableBoundingClientRect.width;
        this.#grabFractionY = (event.clientY - draggableBoundingClientRect.top) / draggableBoundingClientRect.height;

        // Give the drag ghost a heads-up that a drag may be about to start, using the brief idle window
        // between pressing and moving to do any preparation work up front. This is only a "get ready" signal:
        // it must not display anything yet (the drag ghost appears on the first move) and must run synchronously so
        // it does not delay the pointerdown handler. The prepared content is handed over later via getContent.
        this.#dragGhost.dragImminent?.();

        // The drag is not based on the click. Only arm a one-shot pointermove here; the drag starts (drag ghost
        // built, pointer capture bound, .NET side notified) on the first qualifying move, so no drag ghost appears
        // on a plain click.
        this.context.draggable.addEventListener('pointermove', this.#pointerMoveEventListener, { once: true });
    };

    readonly #pointerMoveEventListener = (event: PointerEvent): void => {
        void this.#handlePointerMove(event);
    };

    readonly #handlePointerMove = async (event: PointerEvent) => {
        // Re-check the modifier gate: the state may have changed between pointerdown and the first move.
        if (!this.#hasRequiredKeyState(event))
            return;

        const { visualViewport } = globalThis;
        if (!visualViewport) {
            console.error('Visual Viewport API not supported');

            return;
        }

        this.#dropzoneDescriptors = undefined;
        this.#targetDropzoneDescriptor = undefined;

        // A "ticket" is simply a monotonically increasing number that identifies this one drag. #endDrag
        // bumps #dragTicket, so any async work still referencing an older ticket (e.g. a content-swap
        // notification that arrives from a drag which has already ended) can compare its captured ticket
        // against the current one and bail out. It is a lightweight guard against stale callbacks without
        // having to track and cancel each pending operation individually.
        const ticket = ++this.#dragTicket;

        // Build the drag ghost and bind pointer capture synchronously against this fresh pointermove
        // event. The drag begins on the move, not on the click, so the drag ghost appears only once the pointer
        // actually moves. The composed element is the stable capture target; only its inner content is
        // swapped later, capture is never rebound.
        const dragGhostHost = new DragGhostHost();

        const ghostContent = this.#dragGhost.getContent();

        dragGhostHost.setContent(ghostContent);

        dragGhostHost.appendTo(document.body);

        this.#dragGhostHost = dragGhostHost;

        // Seed the drag ghost's start position before binding capture: PointerCapture.start reads the element's
        // getBoundingClientRect to build the readonly originalRect the behavior pipeline positions from. So
        // this is not a workaround around the pipeline but produces its input — the element must already sit
        // at its intended origin when capture binds. The origin keeps the grabbed fraction under the pointer:
        // the fraction was measured on pointerdown against the dragged element and is applied here at the
        // current pointer position, at the drag ghost's own size. Measured after append so offsetWidth/Height
        // reflect the rendered content (one reflow, accepted). From here the pipeline (originalRect + pointer
        // delta) keeps the fraction under the pointer for the rest of the drag; nothing recurring remains.
        dragGhostHost.setPosition(
            event.clientX - (this.#grabFractionX * dragGhostHost.offsetWidth) + window.scrollX,
            event.clientY - (this.#grabFractionY * dragGhostHost.offsetHeight) + window.scrollY
        );

        const boundingClientRect = new DOMRect(0, 0, visualViewport.width, visualViewport.height);

        this.#pointerCapture = new PointerCapture();
        this.#pointerCapture.onPointerMove = this.#draggableGhostPointerMove;
        this.#pointerCapture.onPointerUp = this.#draggableGhostPointerUp;
        this.#pointerCapture.startedCssClass = this.context.startedCssClass;
        this.#pointerCapture.ongoingCssClass = this.context.ongoingCssClass;
        this.#pointerCapture.endedCssClass = this.context.endedCssClass;
        this.#pointerCapture.behaviors = [...this.#getAllPointerCaptureBehaviors(boundingClientRect)];
        this.#pointerCapture.start(event, dragGhostHost.element, boundingClientRect);

        this.context.draggable.classList.add(this.context.startedCssClass);

        // Wire up "notify-then-pull" content updates: the drag ghost raises contentChanged whenever it has
        // fresher content to show, and we react by pulling that content (getContent) and swapping it into the live
        // drag ghost. This is wired after the drag ghost is already on screen, so a slow content update can never
        // delay the start of the drag. The ticket captured above lets a notification that fires after the drag
        // has ended be ignored (the guard lives in #swapDragGhostContent).
        this.#dragGhost.contentChanged = () => {
            this.#swapDragGhostContent(ticket);
        };

        this.#dragGhost.dragStart?.();

        // Resolve dropzones after capture is bound. A zero-length result means "no valid targets":
        // the drag ghost still drags, but the drop is a no-op (no target is ever hit-tested).
        this.#dropzoneDescriptors = await this.context.dotNetObject.invokeMethodAsync('DragStartAsync', this.context.draggableId);
    };

    readonly #pointerUpEventListener = (_event: PointerEvent) => {
        // Pointer released without moving: cancel the pending drag-start so a plain click never drags.
        this.context.draggable.removeEventListener('pointermove', this.#pointerMoveEventListener);
    };

    // Runs on every pipeline-driven pointer move of the drag ghost. Flips the draggable from its "started" to its
    // "ongoing" CSS state, then hit-tests the drag ghost's current top-left corner against the resolved dropzones
    // to fire enter/leave callbacks as the pointer crosses them. The dropzone rects are shifted by
    // window.scrollX/Y because getBoundingClientRect is viewport-relative, whereas the drag ghost position
    // (captureTarget.rect) is page-relative; without the shift they would disagree once the page is scrolled.
    readonly #draggableGhostPointerMove = (captureTarget: CaptureTarget): void => {
        void this.#handleDraggableGhostPointerMove(captureTarget);
    };

    readonly #handleDraggableGhostPointerMove = async (captureTarget: CaptureTarget) => {
        this.context.draggable.classList.remove(this.context.startedCssClass);
        this.context.draggable.classList.add(this.context.ongoingCssClass);

        const x = captureTarget.rect.left;
        const y = captureTarget.rect.top;

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
            if (this.#targetDropzoneDescriptor) {
                this.#dragGhost.dropzoneLeave?.();

                await this.context.dotNetObject.invokeMethodAsync('DragLeaveAsync', this.#targetDropzoneDescriptor.id);
            }

            this.#targetDropzoneDescriptor = newTargetDropzoneDescriptor;

            if (this.#targetDropzoneDescriptor) {
                this.#dragGhost.dropzoneEnter?.();

                await this.context.dotNetObject.invokeMethodAsync('DragEnterAsync',
                    this.context.draggableId,
                    this.#targetDropzoneDescriptor.id);
            }
        }
    };

    readonly #draggableGhostPointerUp = (captureTarget: CaptureTarget, wasPointerMoved: boolean): void => {
        void this.#handleDraggableGhostPointerUp(captureTarget, wasPointerMoved);
    };

    readonly #handleDraggableGhostPointerUp = async (captureTarget: CaptureTarget, wasPointerMoved: boolean) => {
        let x = captureTarget.rect.left;
        let y = captureTarget.rect.top;

        this.#endDrag();

        this.context.draggable.classList.remove(this.context.startedCssClass);

        if (wasPointerMoved) {
            this.context.draggable.classList.remove(this.context.ongoingCssClass);
            this.context.draggable.classList.add(this.context.endedCssClass);

            await this.context.dotNetObject.invokeMethodAsync('DragEndAsync', this.context.draggableId, x, y);

            if (this.#targetDropzoneDescriptor) {
                const dropzoneBoundingClientRect = this.#targetDropzoneDescriptor.element.getBoundingClientRect();

                x -= dropzoneBoundingClientRect.x + window.scrollX;
                y -= dropzoneBoundingClientRect.y + window.scrollY;

                await this.context.dotNetObject.invokeMethodAsync('DragDroppedAsync',
                    this.context.draggableId,
                    this.#targetDropzoneDescriptor.id,
                    x,
                    y);
            }
        }
    };

    constructor(readonly context: DragInteractionContext, dragGhost: ResolvedDragGhost) {
        this.#dragGhost = dragGhost;
        this.#dragGhost.setDraggable?.(context.draggable);

        for (const behavior of context.pointerCaptureBehaviors ?? [])
            this.#additionalPointerCaptureBehaviors.add(behavior);

        context.draggable.addEventListener('pointerdown', this.#pointerDownEventListener);
        context.draggable.addEventListener('pointerup', this.#pointerUpEventListener);
    }

    #hasRequiredKeyState(event: PointerEvent) {
        if (this.context.modifierKey === null)
            return !event.isModifierKeyPressed();

        // A modifier is configured: the required key state is satisfied while that modifier is held.
        // Note: this only checks the configured modifier, so it is also satisfied when additional keys are held.
        return event.getModifierState(ModifierKey[this.context.modifierKey]);
    }

    #swapDragGhostContent(ticket: number) {
        if (ticket !== this.#dragTicket)
            return;

        const dragGhostHost = this.#dragGhostHost;
        if (!dragGhostHost)
            return;

        const content = this.#dragGhost.getContent();

        const oldSize = { width: dragGhostHost.offsetWidth, height: dragGhostHost.offsetHeight };

        dragGhostHost.setContent(content);

        const newSize = { width: dragGhostHost.offsetWidth, height: dragGhostHost.offsetHeight };

        this.#reanchorDragGhost(oldSize, newSize);
    }

    // Keep the grabbed fraction under the pointer when a differently-sized drag ghost element swaps in. The
    // capture pipeline positions the composed element from the readonly originalRect + pointer delta, so the
    // re-baseline (grab-fraction × size-delta, plus the new size for the movement clamp) is handed to the
    // ReanchorPointerCaptureBehavior instead of mutating originalRect. It takes effect on the next
    // pointermove; re-running the behavior pipeline applies it immediately, so a swap that arrives while the
    // pointer is held does not wait for a move.
    #reanchorDragGhost(oldSize: { width: number; height: number }, newSize: { width: number; height: number }) {
        const { width: oldWidth, height: oldHeight } = oldSize;
        const { width: newWidth, height: newHeight } = newSize;

        if (oldWidth === newWidth && oldHeight === newHeight)
            return;

        const shiftX = this.#grabFractionX * (oldWidth - newWidth);
        const shiftY = this.#grabFractionY * (oldHeight - newHeight);

        this.#reanchorPointerCaptureBehavior.reanchor(shiftX, shiftY, newWidth, newHeight);

        this.#pointerCapture?.applyBehaviors();
    }

    #endDrag() {
        this.#dragTicket++;

        this.#dragGhost.dragEnd?.();

        this.#dragGhost.contentChanged = undefined;

        this.#dragGhostHost?.remove();

        this.#dragGhostHost = undefined;
        this.#pointerCapture = undefined;
    }

    * #getAllPointerCaptureBehaviors(boundingClientRect: DOMRect): IterableIterator<PointerCaptureBehavior> {
        this.#movePointerCaptureBehavior.initialize(boundingClientRect);
        yield this.#movePointerCaptureBehavior;

        this.#reanchorPointerCaptureBehavior.initialize(boundingClientRect);
        yield this.#reanchorPointerCaptureBehavior;

        if (this.#additionalPointerCaptureBehaviors.size > 0)
            yield new AggregatePointerCaptureBehavior(this.#additionalPointerCaptureBehaviors);

        yield this.#adjustForScrollPositionPointerCaptureBehavior;
        yield this.#setPositionPointerCaptureBehavior;
    }

    public dispose() {
        this.context.draggable.removeEventListener('pointerdown', this.#pointerDownEventListener);
        this.context.draggable.removeEventListener('pointerup', this.#pointerUpEventListener);
        this.context.draggable.removeEventListener('pointermove', this.#pointerMoveEventListener);

        this.#endDrag();

        // Release the draggable link once, when the interaction is torn down. The link is stable across
        // drags, so it must survive #endDrag (which runs after every drag) to keep repeated drags working.
        this.#dragGhost.clearDraggable?.();
    }

    public addPointerCaptureBehavior(pointerCaptureBehavior: PointerCaptureBehavior) {
        this.#additionalPointerCaptureBehaviors.add(pointerCaptureBehavior);
    }

    public removePointerCaptureBehavior(pointerCaptureBehavior: PointerCaptureBehavior) {
        this.#additionalPointerCaptureBehaviors.delete(pointerCaptureBehavior);
    }
}

async function resolveDragGhost(jsModule: DragGhostJsModuleDescriptor | undefined): Promise<ResolvedDragGhost> {
    if (!jsModule)
        return new DefaultDragGhost();

    try {
        // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
        const module = await import(jsModule.moduleName);

        // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access, @typescript-eslint/no-unsafe-call
        return module[jsModule.createFunction.name](jsModule.createFunction.args) as ResolvedDragGhost;

    } catch (error) {
        console.error(`Failed to resolve drag ghost from '${jsModule.moduleName}'; falling back to the default drag ghost.`, error);

        return new DefaultDragGhost();
    }
}

export async function attach(context: DragInteractionContext) {
    const dragGhost = await resolveDragGhost(context.dragGhostJsModule ?? undefined);

    return new DragInteraction(context, dragGhost);
}
