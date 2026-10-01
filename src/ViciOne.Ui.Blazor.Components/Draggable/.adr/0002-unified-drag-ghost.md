# Unified drag ghost

## Status

Accepted (refines component-level [0001](./0001-show-drag-ghost-without-dropzone.md))

## Context

The drag ghost is one responsibility — *what the visual ghost element is, kept current* — under two hard forces:

- **Latency (0001).** Pointer capture must bind to the live pointer event that starts the drag (the first `pointermove` after a valid `pointerdown`) synchronously, before any round-trip; binding after an awaited round-trip binds a stale event and loses the gesture. Any "render on demand" path is async under Interactive Server Rendering (a SignalR round-trip) and cannot sit on the critical path.
- **Scale + laziness.** A hidden Blazor subtree *per draggable, eager at attach* collapses on large collections (thousands of rows → thousands of component instances + hidden DOM). Homogeneous collections (table rows) must need no per-row host — one live-element ghost suffices; a markup ghost is placed **once**, render-stable, and shared.

## Decision

### One flow, always down to JavaScript, one ghost authority

C# never branches on ghost strategy. Every draggable resolves to at most one **drag ghost** on the JavaScript side — the sole authority for what the ghost element is and how it stays current. `DragInteraction` (both sides) is kind-blind: it neither knows nor exposes whether a drag ghost is backed by a Blazor component, live-DOM cloning, or anything else.

### C# consumer surface

`AttachAsync(draggable, …, IDragGhost? dragGhost = null)` — `null` → default drag ghost.

```csharp
public interface IDragGhost
{
    DragGhostJsModuleDescriptor GetJsModule();   // front door: called by AttachAsync, once, at attach
}

public sealed record DragGhostJsModuleDescriptor   // wire format: travels to JavaScript
{
    public required string ModuleName { get; init; }            // ES module exporting createDragGhost
    public required JsFunctionDescriptor CreateFunction { get; init; }   // factory name + optional interop args
}
```

`AttachAsync` calls `GetJsModule()` once and copies the descriptor into the JavaScript interaction context verbatim. JavaScript imports `ModuleName` once per attach and calls `CreateFunction.Name` (`createDragGhost`) with `CreateFunction.Args` to get the instance; JavaScript owns its full lifecycle (create at attach, drop at detach).

- **One method, kind-blind.** Extension happens through `CreateFunction.Args` (non-breaking record growth), never by widening the interface — no sub-kinds, no type-switch.
- **Late binding.** Built at attach (typically `OnAfterRenderAsync`, host rendered), so it resolves volatile state like the component's `ElementReference`.
- **Method, not property** — creation may have side effects (a component drag ghost lazily creates its `DotNetObjectReference`).
- A throwing `GetJsModule()` (or a failed module import) is caught in the attach (log + degrade to default drag ghost). This is the *single* sliver of ghost handling in `DragInteraction`, and it is kind-blind.
- `CreateFunction.Args` may carry anything the interop serializer handles — including `ElementReference` and `DotNetObjectReference`, which revive JavaScript-side to a live DOM element / callback object (argument-direction revival — proven in-repo, one-level nesting via `DragInteractionContext`).
- A **pure-JavaScript drag ghost is one `.ts` file** plus an `IDragGhost` class holding the module descriptor (e.g. `ITableRowDragGhost`): no interop, no `IJSObjectReference` bookkeeping, no C#-side disposal.

### JavaScript runtime contract

The instance returned by `createDragGhost` is a `ResolvedDragGhost`: the required content source plus any opt-in details, feature-detected by `DragInteraction`.

```ts
interface DragGhostContentSource {
  getContent(): HTMLElement;   // sync, critical path; the ghost element's content
}

// opt-in, feature-detected on the instance:
//   setDraggable(draggable) / clearDraggable()  — ghosts that build from the dragged element
//   contentChanged?: () => void                 — DragInteraction assigns it; the ghost raises it to signal new content
//   dragImminent()                              — sync, before getContent
//   dragStart() / dragEnd() / dropzoneEnter() / dropzoneLeave()
```

`createDragGhost(args)` returns a `ResolvedDragGhost`.

### JavaScript: stable host, sync capture, notify-then-pull updates

On `pointerdown` (after the synchronous suppression guards) `DragInteraction` only **warms** the drag ghost: it captures the grab anchor, calls `dragImminent()` (synchronous content prep), and arms a one-shot `pointermove`. The drag actually begins on that first `pointermove`, where `DragInteraction`:

1. creates the **ghost host** ([`DragGhostHost`](../Scripts/DragGhostHost.ts)) on demand (one per drag, removed on drag end),
2. reads `getContent()`, mounts it, and appends the host to the document,
3. binds pointer capture to the host **synchronously** against the live `pointermove` (preserving 0001),
4. calls `dragStart()` and resolves dropzones via `DragStartAsync`.

Starting the drag on the first move (not the press) means a plain click never mounts a ghost; a `pointerup` before any move cancels the armed `pointermove`.

The host is a **stable capture target**: inner content is swapped later, capture never rebound. `.draggable-*` state and positioning styles live on the host; the inner content keeps its own layout. Every async step stays off the critical path.

- Updates are **notify-then-pull.** `DragInteraction` assigns `contentChanged` on the ghost; the ghost raises it to signal fresh content is available, and `DragInteraction` re-fetches via `getContent()` and swaps the host's inner content.
- A **drag ticket** guards currency: bumped on drag end, so a late `contentChanged` (fast flick, disposal, rapid re-drag) is a no-op. It lives **inside `DragInteraction`** — drag ghosts never see it. **Contract: any `contentChanged` after the drag ends is a guaranteed no-op; drag ghosts need no currency guards.** Ticket bumps and stale-notification drops are handled at the swap site.
- On each swap the host is **re-anchored** from the stored grab fraction and old/new content dimensions, so a differently-sized ghost doesn't jump off the pointer. `ReanchorPointerCaptureBehavior` re-baselines the frozen capture origin + size mid-drag (grab-fraction × size-delta; no pointer-coordinate plumbing) from the readonly `originalRect`, leaving that intentionally-readonly source untouched.
- `DragInteraction` **never waits** for the drag ghost and needs no completion signal — mount sync initial, swap on each `contentChanged`, drag ends on `pointerup` regardless.
- **Ghost is cosmetic (0001):** drag-ghost failure = no swap (the ghost logs its own error), last content stays. A failed module import at attach → default drag ghost. The drag is never blocked.
- **Initial-sync and mid-drag updates share one path.** The sync initial (`getContent`) is distinct only because it must not round-trip; everything after is `contentChanged` → `getContent`, indistinguishable to `DragInteraction`.

### The markup path: one abstract Blazor base component

The markup-authored ghost is **one drag ghost like any other** — a library `.ts` module ([`DragGhostBase.cs.ts`](../Components/DragGhostBase.cs.ts)) plus the abstract [`DragGhostBase`](../Components/DragGhostBase.cs) a consumer subclasses. `DragInteraction` has no markup knowledge.

- **`DragGhostBase : ComponentBase, IDragGhost, IDisposable`.** The consumer subclasses it in a `.razor` file, renders a [`DragGhostContent`](../Components/DragGhostContent.razor) shell bound via `@ref="Content"`, and passes the component (`@ref`) to `AttachAsync`. It *is* the `IDragGhost`; owns its `[JSInvokable] Forward*` methods, its `DotNetObjectReference`, and the off-screen `DragGhostContent` element. `GetJsModule()` packs the ref + content element + opt-in flags into typed `CreateDragGhostArgs`.
- **Content is declarative `ChildContent`** on the `DragGhostContent` shell:

  ```razor
  @inherits DragGhostBase

  <DragGhostContent @ref="Content">
      @* ghost markup *@
  </DragGhostContent>
  ```

  `DragGhostContent` renders its `ChildContent` on the **first** Blazor render cycle, so the off-screen shell is ready before the first drag (no first-drag default-ghost penalty). The host is off-screen so its content has real layout but is never visible. Every later render goes through `RenderAsync`, which awaits the flush (`TaskCompletionSource` + `OnAfterRender`) and raises `Rendered`.
- **Lifecycle opt-ins.** A subclass implements `IDragStartListener`, `IDragEndListener`, `IDropzoneEnterListener` and / or `IDropzoneLeaveListener`. On a matching callback, JavaScript invokes `ForwardXxxAsync`, which clears a render flag, awaits the listener (which mutates state and calls `RenderContentAsync()` → `Content.RenderAsync()` awaits the flush), and returns whether a render happened. JavaScript raises `contentChanged` only when it did — **except `dragEnd`**, which forwards the tick (to refresh the off-screen content for the next drag) but drops the result, since the ghost is being torn down.
- **`getContent`** clones the content element (`cloneNode(true)`) and strips component-core identity (`drag-ghost-content` class, `_bl_`/`b-` attributes), so the ghost carries no styling or identity from its source element.
- **Opt-in wiring.** `createDragGhost` always exposes the lifecycle methods, but each gates itself on the flag captured at construction (`ProcessDragStart`, …), so an unimplemented callback is a no-op and makes no round-trip. `DragInteraction` additionally feature-detects the callbacks on the instance.
- **Guards.** `Content` null at `GetJsModule()` → throw (caught in attach → degrade to default). Render after dispose → guarded no-op; `Dispose` releases the `DotNetObjectReference`.
- **Render-stability constraint.** Must be placed in render-stable markup. Inside conditional markup that unmounts it, the host element dies and JavaScript silently degrades to default. Documented on the component.

### Granularity, laziness

- Stateless ghosts reading the live element (table row) are naturally **shared**: one module, one instance per attach, no state.
- Stateful ghosts (`DragGhostBase`) are **per instance**, consumer-scoped — typically one per distinct ghost appearance, placed once in render-stable markup.
- A markup ghost renders its off-screen shell in the normal render cycle and refreshes it per lifecycle callback via `RenderContentAsync()`.

## Seams left open (deliberately non-breaking)

- **Warming** — a ghost may pre-run async work (e.g. on hover) so `getContent` hits sync. No interface change.
- **Mid-drag / config updates** — a C#-originated change can mutate state and raise `contentChanged` via the live notifier. The host + `contentChanged` already support n updates; `DragInteraction` stays uninvolved.
- **Host lookup by selector** — an optional selector/id in `CreateFunction.Args` (JavaScript resolves `element ?? querySelector(selector)` per drag) for placements that can't be render-stable. Additive both sides; **not** built now.

## Consequences

- `DragInteraction` carries **no ghost logic in C#** beyond one kind-blind step (call `GetJsModule()`, catch + log + degrade), and **no** per-ghost special-casing in TS beyond the generic import/instantiate + feature-detect.
- Consumer places a `DragGhostBase` subclass only for markup ghosts, anywhere render-stable, passed by `@ref` — no per-draggable hosts, no app-root convention, no registry. Table rows need no component at all (`ITableRowDragGhost`, DI-registered by `AddDraggable()`).
- The off-screen shell renders on the first cycle, so a markup ghost shows its real content from the **first** drag; the table-row ghost's sync `getContent` never pops.
- DOM shape under the drag lives inside the `DragGhostHost` (positioning + state classes on the host, content nested inside). Existing styles that assumed a different ghost DOM shape may break; accepted, no pre-audit.
- Module paths become a runtime contract (`/_content/{assembly}/…`): a wrong path fails at attach with a console error + default ghost, not at compile time. Static descriptor factories keep the strings library-owned.
- Build-time verification: revival of `ElementReference` + `DotNetObjectReference` nested in typed `CreateFunction.Args` (argument direction).
- `pointercancel` is **out of scope**: the shared `PointerCapture` module ends a drag only on `pointerup`, so a canceled touch gesture leaves the host orphaned — a pre-existing shared-module leak, deferred.

## Alternatives considered

- **Marker interface + typed sub-kinds resolved by a type-switch in `AttachAsync`.** Rejected: the C# loop-through is ceremony for pure-JavaScript ghosts; C# would hold and dispose `IJSObjectReference`s; the type-switch + context flag leak the split into `DragInteraction`.
- **Async `getContent` / single method returning a `Promise`.** Rejected: forces an `await` before pointer-capture binding → the stale-event gesture loss 0001 prevents.
- **Push the element (`emit(content)`) instead of notify-then-pull.** Superseded by `contentChanged` + `getContent`: the notifier carries no payload, so the same pull path serves the sync initial and every update, and the ghost owns when its content is materialized.
- **`ElementReference` as a `[JSInvokable]` return value.** Rejected: return-direction revival is unproven; passing the element once in `CreateFunction.Args` uses the proven argument direction. Recreation tolerance is the deferred selector seam.
- **Lazy `_render` gate on the markup shell (render only on first drag).** Rejected: `DragGhostContent` renders off-screen in the normal cycle, so the shell is ready before the first drag with no extra round-trip and no first-drag default-ghost penalty.
- **Global library-owned host + registry.** Rejected: forces `DragInteraction` to know the markup path exists (render callback + registry + missing-host guards), adds a service and a layout convention, makes misconfiguration a runtime warning.
- **Pull-only (re-pull on events `DragInteraction` sees).** Can't deliver updates whose trigger is invisible to `DragInteraction` (a C#-originated config change). The `contentChanged` notify is the correct primitive.
- **JavaScript creates the host, passes a ref up for C# to render into.** Infeasible under Blazor Server: a `RenderFragment` renders only through Blazor's component tree, not into a detached node JavaScript created.
- **No DOM host: `HtmlRenderer` → HTML string → `innerHTML`.** Deferred, not rejected: static-render semantics (no interactive children) and DI-scope/dispatcher divergence need their own investigation.
