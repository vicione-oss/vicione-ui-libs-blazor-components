# Use SemaphoreSlim with Reentrant AsyncLocal-Based Lock

## Status

Accepted

## Initial Question

In `DragInteraction.cs` we are using a semaphore in all async methods to avoid concurrent issues.

But when code, that is called from `DragInteraction`, executes an async method of `DragInteraction` like `RemoveAsync()`, then a dead-lock occurs.

Can the synchronization mechanisms be changed in a way that concurrency is still not an issue and possible dead-locks are avoided?

## Problem Analysis

`DragInteraction` uses a `SemaphoreSlim(1, 1)` as an async mutual-exclusion lock. All public async
methods acquire this semaphore via `WithSemaphoreAsync`. However, several methods (`DragDroppedAsync`,
`DragEndAsync`, `DragEnterAsync`, `DragLeaveAsync`) call external `IDropzone` callbacks while holding
the lock. These callbacks can call back into `DragInteraction` (e.g., `RemoveAsync`), causing a
deadlock because the semaphore is already held and is non-reentrant.

### Concrete Deadlock Path

1. JS invokes `DragDroppedAsync` → `WithSemaphoreAsync` acquires semaphore
2. Inside lock: calls `dropzone.DragDroppedAsync(draggable, x, y)`
3. Dropzone delegates to `DropHandler.DragDroppedAsync`
4. Drop handler calls `dragInteraction.RemoveAsync(draggable)`
5. `RemoveAsync` calls `WithSemaphoreAsync` → tries to acquire same semaphore → **DEADLOCK**

### Example: DropHandler triggering the deadlock

```csharp
public async Task DragDroppedAsync(IDraggable draggable, double x, double y, IDropzone target)
{
    await dragInteraction.RemoveAsync(draggable); // ← re-enters DragInteraction while lock is held

    ...
}
```

## Solution

Add an `AsyncLocal<bool>` field to track whether the current async flow already holds the lock.
When re-entering from within a locked callback, skip semaphore acquisition and execute inline.

### Why AsyncLocal?

`AsyncLocal<T>` stores its value in the `ExecutionContext`. Key behaviors:

- **Synchronous calls** (no `async`/`await`) don't create a new `ExecutionContext` — they execute
  on the **same** context as the caller. So `_semaphoreAcquired.Value` remains `true` throughout the
  synchronous call chain.
- **Async calls** (with `async`/`await`) capture the current `ExecutionContext` at the start.
  The child sees the parent's `AsyncLocal` values. Copy-on-write only matters for changes flowing
  **back up** — but we only need the value to flow **down**.
- Truly concurrent calls from different origins get their own `ExecutionContext`, so they still
  serialize properly through the semaphore.

## Q&A

### Does this still work when the implementation of `dropzone.DragDroppedAsync()` does not use async/await keywords (e.g. to optimize away the async state machine)?

**Yes, it still works.** `AsyncLocal<T>` stores its value in the `ExecutionContext`.

- **Synchronous calls** (no `async`/`await`) don't create a new `ExecutionContext` — they execute
  on the **same** context as the caller. So `_semaphoreAcquired.Value` remains `true` throughout the
  synchronous call chain.
- **Async calls** (with `async`/`await`) capture the current `ExecutionContext` at the start.
  The child sees the parent's `AsyncLocal` values. Copy-on-write only matters for changes flowing
  **back up** — but we only need the value to flow **down**.

Tracing the non-async scenario concretely:

```
WithSemaphoreAsync:                          // async — captures ExecutionContext
  _semaphoreAcquired.Value = true            // set on current context
  await action()
    → dropzone.DragDroppedAsync(...)         // NON-async — same ExecutionContext
      → dropHandler.DragDroppedAsync(...)    // NON-async — same ExecutionContext
        → dragInteraction.RemoveAsync(...)   // async — captures current context
          → WithSemaphoreAsync
            → _semaphoreAcquired.Value == true ✓  // still visible
            → executes inline, no semaphore
```

No `ExecutionContext` boundary is crossed between setting the flag and checking it, regardless of
how many non-async methods sit in between. The value flows down through both sync and async call
paths.

The only scenario where `AsyncLocal` would **not** work is if someone explicitly used
`ExecutionContext.SuppressFlow()` or spawned work on a separate thread pool thread via `Task.Run` —
but neither is expected in this Blazor component callback model.

## Implementation Details

### Key Code: WithSemaphoreAsync (reentrant version)

```csharp
private readonly AsyncLocal<bool> _semaphoreAcquired = new();

...

private async Task WithSemaphoreAsync(Func<Task> action)
{
    try
    {
        if (_semaphoreAcquired.Value)
        {
            await action();

            return;
        }

        await _semaphore.WaitAsync(_cancellationTokenSource.Token);
        try
        {
            _semaphoreAcquired.Value = true;

            await action();
        }
        finally
        {
            _semaphoreAcquired.Value = false;

            _semaphore.Release();
        }
    }
    catch (OperationCanceledException)
    {
        // Nothing to do here, return gracefully
    }
    catch (ObjectDisposedException)
    {
        // Semaphore or CancellationTokenSource already disposed, nothing we can do, return gracefully
    }
}
```
