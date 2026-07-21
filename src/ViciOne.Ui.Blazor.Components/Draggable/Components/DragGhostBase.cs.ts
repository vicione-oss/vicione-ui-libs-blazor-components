import { type DragGhostContentSource } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-ghost-content-source.js';
import { type DragGhostContentChangedNotifier } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-ghost-content-changed-notifier.js';
import { type DragStartListener } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-start-listener.js';
import { type DragEndListener } from '/_content/ViciOne.Ui.Blazor.Components/draggable/drag-end-listener.js';
import { type DropzoneEnterListener } from '/_content/ViciOne.Ui.Blazor.Components/draggable/dropzone-enter-listener.js';
import { type DropzoneLeaveListener } from '/_content/ViciOne.Ui.Blazor.Components/draggable/dropzone-leave-listener.js';
import { type CreateDragGhostArgs } from '/_content/ViciOne.Ui.Blazor.Components/draggable/models/create-drag-ghost-args.js';

/**
 * The single stateful drag ghost backing a `DragGhostBase` subclass.
 *
 * Holds the host element and .NET object reference for the whole drag, so `getContent` and the lifecycle
 * callbacks share one instance. `getContent` returns the class-stripped element clone; `dragStart`, `dragEnd`,
 * `dropzoneEnter` and `dropzoneLeave` forward their ticks to the .NET listener, which renders the content
 * itself when it has something new to show.
 *
 * Content changes — whether rendered from a lifecycle callback or from outside one (for example a per-second
 * timer) — reach the drag ghost the same way: rather than have .NET call into JavaScript, the drag ghost pulls.
 * While `contentChanged` is assigned (which `DragInteraction` does for the duration of the drag) it keeps a
 * `WaitForContentChangeAsync` interop call pending on the .NET drag ghost and raises `contentChanged` each time it
 * resolves. This keeps the drag ghost driven entirely by JavaScript calling into .NET, never the other way around.
 *
 * The lifecycle methods are always present, but each one is gated by the opt-in flag captured at construction:
 * when the deriving component did not implement the matching listener interface, the method is a no-op and makes
 * no round-trip.
 */
class DragGhostBase implements DragGhostContentSource, DragGhostContentChangedNotifier, DragStartListener, DragEndListener, DropzoneEnterListener, DropzoneLeaveListener {
    #contentChanged?: () => void;
    #waitingForContentChanges = false;

    readonly #contentElementReference: HTMLElement;
    readonly #dotNetObject: DotNet.DotNetObject;
    readonly #processDragStart: boolean;
    readonly #processDragEnd: boolean;
    readonly #processDropzoneEnter: boolean;
    readonly #processDropzoneLeave: boolean;

    constructor(args: CreateDragGhostArgs) {
        this.#contentElementReference = args.contentElementReference;
        this.#dotNetObject = args.dotNetObject;
        this.#processDragStart = args.processDragStart;
        this.#processDragEnd = args.processDragEnd;
        this.#processDropzoneEnter = args.processDropzoneEnter;
        this.#processDropzoneLeave = args.processDropzoneLeave;
    }

    /**
     * Assigned by `DragInteraction` for the duration of a drag and cleared (set to `undefined`) when the drag
     * ends. Assigning a handler starts a pull loop that awaits the next out-of-band content change on the .NET
     * drag ghost; clearing it stops the loop.
     */
    public get contentChanged(): (() => void) | undefined {
        return this.#contentChanged;
    }

    public set contentChanged(value: (() => void) | undefined) {
        this.#contentChanged = value;

        if (value !== undefined && !this.#waitingForContentChanges)
            void this.#waitForContentChanges();
    }

    public getContent(): HTMLElement {
        return this.#cloneContent();
    }

    public dragStart() {
        if (!this.#processDragStart)
            return;

        // Forward the drag-start tick to the .NET listener. If it renders content (via RenderContentAsync),
        // that completes the pending content-change wait, which swaps the fresh content into the drag ghost.
        // Runs off the critical path — the drag ghost never blocks the drag.
        void this.#invoke('ForwardDragStartAsync');
    }

    public dragEnd() {
        if (!this.#processDragEnd)
            return;

        // The drag ghost element is torn down as the drag ends, so there is nothing to swap into. The .NET listener
        // still renders the off-screen content for the next drag; we just forward the tick.
        void this.#invoke('ForwardDragEndAsync');
    }

    public dropzoneEnter() {
        if (!this.#processDropzoneEnter)
            return;

        // Forward the dropzone-enter tick to the .NET listener; any render it performs completes the pending
        // content-change wait, swapping the fresh content into the drag ghost. Runs off the critical path.
        void this.#invoke('ForwardDropzoneEnterAsync');
    }

    public dropzoneLeave() {
        if (!this.#processDropzoneLeave)
            return;

        // Forward the dropzone-leave tick to the .NET listener; any render it performs completes the pending
        // content-change wait, swapping the fresh content into the drag ghost. Runs off the critical path.
        void this.#invoke('ForwardDropzoneLeaveAsync');
    }

    #cloneContent(): HTMLElement {
        // eslint-disable-next-line @typescript-eslint/no-unsafe-type-assertion
        const result = this.#contentElementReference.cloneNode(true) as HTMLElement;

        // Strip Blazor's bookkeeping so the clone is a plain, standalone element: `drag-ghost-content` is the
        // marker class on the source, `_bl_*` are Blazor's element-reference ids, `b-*` are the scoped-CSS
        // markers that tie the element to its owning component's styles, and an empty `class=""` is leftover
        // noise. Removing them means the drag ghost copy carries no styling or identity back from its source.
        result.classList.remove('drag-ghost-content');

        Array.from(result.attributes)
            .filter(attr => attr.name.startsWith('_bl_') || attr.name.startsWith('b-') || (attr.name === 'class' && attr.value === ''))
            .forEach(attr => result.removeAttributeNode(attr));

        return result;
    }

    /**
     * Pull loop for out-of-band content changes (for example a per-second timer). While `contentChanged` is
     * assigned it keeps a single `WaitForContentChangeAsync` call pending on the .NET drag ghost; each time that
     * resolves with `true` a change was rendered, so we raise `contentChanged` and wait again. A `false` result
     * means the drag ghost was disposed, so the loop stops. The .NET side keeps at most one waiter, so this never
     * piles up calls.
     */
    async #waitForContentChanges(): Promise<void> {
        this.#waitingForContentChanges = true;

        try {
            while (this.#contentChanged !== undefined) {
                // Intentional sequential await: the .NET drag ghost keeps at most one pending waiter, so each
                // WaitForContentChangeAsync must resolve before the next is issued (no parallelism possible).
                // eslint-disable-next-line no-await-in-loop
                const changed = await this.#dotNetObject.invokeMethodAsync<boolean>('WaitForContentChangeAsync');

                if (!changed)
                    return;

                this.#contentChanged?.();
            }
        } catch (error) {
            console.error('DragGhost failed to await the next content change.', error);
        } finally {
            this.#waitingForContentChanges = false;
        }
    }

    /**
     * Forwards a lifecycle tick to the .NET listener. The drag ghost is cosmetic and must never block the drag, so
     * the caller runs this off the critical path. Any content the listener renders completes the pending
     * content-change wait (see #waitForContentChanges), which re-fetches and swaps the drag ghost content, so there
     * is nothing to report back here.
     */
    async #invoke(methodIdentifier: string): Promise<void> {
        try {
            await this.#dotNetObject.invokeMethodAsync(methodIdentifier);

        } catch (error) {
            console.error(`DragGhost failed to invoke ${methodIdentifier}.`, error);
        }
    }
}

/**
 * Creates the single stateful drag ghost for a `DragGhostBase` subclass. The returned instance
 * always exposes the lifecycle methods, but each gates itself on its opt-in flag, so an unimplemented callback
 * makes no round-trip.
 */
export function createDragGhost(args: CreateDragGhostArgs): DragGhostBase {
    return new DragGhostBase(args);
}
