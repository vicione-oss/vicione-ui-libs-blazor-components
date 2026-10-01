# Draggables prepare before dropzones decide

## Status

Accepted

## Context

A dropzone decides in its `DragStart` handler whether to take part, often from what the draggable carries. Some
draggables only settle that state once the drag starts, possibly asynchronously: a table row selects the pressed
row and resolves its payload through the consumer's callbacks. `DragStart` is a synchronous event, so a draggable
could only start that work there and let it finish later. Every dropzone then decided on the state of the previous
drag, or on none.

## Decision

`IDraggable` gets `Task PrepareDragStartAsync()`, with a default implementation that does nothing. `DragStartAsync`
awaits it before raising `DragStart`, for every draggable, even when nothing listens to the event.

A default interface method, rather than a separate opt-in interface: every draggable has the hook, nothing outside
the library has to change, and `DragInteraction` needs no type test.

## Consequences

- Dropzones and drop policies may decide on the draggable's state at drag start.
- The drag itself is not delayed. Per [0001](0001-show-drag-ghost-without-dropzone.md), the ghost and pointer
  capture are live before `DragStartAsync`, so a slow preparation only delays when the dropzones become known.
- A failing preparation is logged, and the dropzones are resolved against the unprepared draggable.
- The preparation runs inside `DragInteraction`'s lock. Anything it triggers that calls back into `DragInteraction`
  in the same async flow re-enters the lock, as the root `.adr/0001` intends.
- First default interface method in the library. It is only callable through `IDraggable`, which is how
  `DragInteraction` holds every draggable.
