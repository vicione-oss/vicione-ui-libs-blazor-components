# Show drag ghost without dropzone

## Status

Accepted

## Context

When the drag begins, `DragInteraction` must both capture the pointer and resolve valid dropzones via `DragStartAsync`. These have conflicting timing:

- **Pointer capture** must bind to the live pointer event that starts the drag (the first `pointermove` after a valid `pointerdown`). Binding after an awaited round-trip binds a stale event, losing the gesture under latency.
- **Dropzone resolution** is a server call — not available synchronously in that event.

They cannot share one "wait, then start" sequence without sacrificing capture.

> The drag is not based on the click: `pointerdown` only warms the ghost (`dragImminent()`), captures the grab anchor, and arms a one-shot `pointermove`; the ghost is mounted and the pointer captured on that first move, so a plain click never produces a ghost.

## Decision

Build the ghost and capture the pointer **synchronously on the first `pointermove`**; resolve dropzones after. Dropzones gate the **drop**, not the **drag**.

A drag with no valid drop target is a **legal state**: the ghost tracks the pointer and the drop is a no-op. The user grabbed the element, so it moves; whether anything accepts it is answered separately, later.

The ghost is **cosmetic** — if a custom drag ghost fails to build, it degrades to a plain copy rather than aborting.

## Consequences

- Capture always binds the fresh `pointermove` event that starts the drag; gesture never lost to latency.
- An element is draggable wherever attached, independent of dropzones. No matching dropzone → ghost still tracks, drop does nothing.
- "Can the drop succeed?" (resolved dropzones, at drop time) is decoupled from "can the user pick it up?".

## Alternatives considered

- **Gate the drag on dropzones** — resolve first, capture only if ≥1 returned. Rejected: forces an awaited round-trip before capture, binding a stale event and dropping the gesture under latency.
- **Start the drag on `pointerdown`** — mount the ghost and bind capture on the press. Rejected: a plain click (press without move) would flash a ghost; starting on the first move avoids this while still binding a live event synchronously.
- **Build ghost first, abort if zero dropzones** — capture, then tear down if server returns none. Rejected: cancels an interaction already begun; reads as the element "sticking".
