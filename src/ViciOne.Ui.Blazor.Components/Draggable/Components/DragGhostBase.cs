using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Draggable.Abstractions;
using ViciOne.Ui.Blazor.Components.Draggable.Models;

namespace ViciOne.Ui.Blazor.Components.Draggable.Components;

/// <summary>
/// Base class for a drag ghost.
/// </summary>
/// <remarks>
/// <para>
///     Derive from this class in a <c>.razor</c> file, render a <see cref="DragGhostContent"/>, bind it via <c>@ref="Content"</c>.
/// </para>
/// <para>
///     Place the deriving component in <b>render-stable</b> markup and pass its (<c>@ref</c>) to <c>AttachAsync</c>.
///     The passed component instance must be valid until <c>RemoveAsync</c> is called.
/// </para>
/// <para>
///     Enrich the drag ghost with additional details by implementing <see cref="IDragStartListener"/>,
///     <see cref="IDragEndListener"/>, <see cref="IDropzoneEnterListener"/> and / or
///     <see cref="IDropzoneLeaveListener"/>. From those callbacks, mutate the state the drag ghost content depends on and
///     call <see cref="RenderContentAsync"/> to refresh the drag ghost.
/// </para>
/// <para>
///     The cursor shown during a drag is the CSS <c>cursor</c> of the first element inside
///     <see cref="DragGhostContent"/>, and follows it whenever <see cref="RenderContentAsync"/> refreshes the drag
///     ghost. Without a cursor of its own, the document's cursor stays.
/// </para>
/// </remarks>
public abstract partial class DragGhostBase : ComponentBase, IDragGhost, IDisposable
{
    private readonly Lock _contentChangeLock = new();

    private DotNetObjectReference<DragGhostBase>? _dotNetObjectReference;
    private DragGhostJsModuleDescriptor? _jsModuleDescriptor;
    private DragGhostContent? _content;
    private TaskCompletionSource<bool>? _contentChangeWaiter;
    private bool _contentChangePending;
    private bool _disposed;

    /// <summary>
    /// The <see cref="DragGhostContent"/> rendered by the deriving component. Call
    /// <see cref="RenderContentAsync"/> to refresh the drag ghost content.
    /// </summary>
    protected DragGhostContent? Content
    {
        get => _content;
        set
        {
            if (_content == value)
                return;

            if (_content is { } previous)
                previous.Rendered -= OnContentRendered;

            _content = value;

            if (_content is { } current)
                current.Rendered += OnContentRendered;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the interop resources held by the drag ghost.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> when called from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;

        if (disposing)
        {
            Content = null;

            _dotNetObjectReference?.Dispose();
            _dotNetObjectReference = null;

            // Release a JavaScript drag ghost that is currently awaiting the next content change, so its interop call
            // completes instead of hanging until the circuit tears down.
            lock (_contentChangeLock)
            {
                _contentChangeWaiter?.TrySetResult(false);
                _contentChangeWaiter = null;
            }
        }
    }

    /// <summary>
    /// Re-renders the <see cref="DragGhostContent"/> and awaits the render flush.
    /// Call this after mutating state the drag ghost content depends on — from a lifecycle listener or from
    /// anywhere else (for example a timer).
    /// <para>
    ///     After the render, the content is swapped into the drag ghost.
    /// </para>
    /// <para>
    ///     When a drag is in progress the render completes the JavaScript drag ghost's pending
    ///     <see cref="WaitForContentChangeAsync"/> call, so the drag ghost swaps the fresh content and immediately
    ///     waits for the next change. Otherwise only the off-screen content is refreshed for the next drag.
    /// </para>
    /// </summary>
    protected async Task RenderContentAsync()
    {
        if (_disposed || Content is null)
            return;

        await Content.RenderAsync();
    }

    /// <summary>
    /// Handles a completed <see cref="DragGhostContent.RenderAsync"/> flush by signalling the JavaScript drag ghost's
    /// pending wait, so it re-fetches and swaps the fresh content into the drag ghost.
    /// </summary>
    private void OnContentRendered()
        => SignalContentChange();

    /// <summary>
    /// Completes the JavaScript drag ghost's pending <see cref="WaitForContentChangeAsync"/> call, or records that a
    /// change is pending when no wait is currently outstanding, so the next wait returns immediately.
    /// </summary>
    private void SignalContentChange()
    {
        lock (_contentChangeLock)
        {
            if (_contentChangeWaiter is { } waiter)
            {
                _contentChangeWaiter = null;

                waiter.TrySetResult(true);
            }
            else
            {
                _contentChangePending = true;
            }
        }
    }

    /// <summary>
    /// Awaited by the JavaScript drag ghost while a drag is in progress. Completes with <see langword="true"/> when
    /// content (for example from a lifecycle listener or a timer tick) is rendered, prompting the drag ghost to
    /// re-fetch and swap the content and then wait again; completes with <see langword="false"/> when the drag ghost
    /// is disposed so the JavaScript loop can stop. Keeping this a TS-initiated call means the drag ghost element is only ever
    /// driven by JavaScript calling into .NET, never the other way around.
    /// </summary>
    [JSInvokable]
    public Task<bool> WaitForContentChangeAsync()
    {
        lock (_contentChangeLock)
        {
            if (_disposed)
                return Task.FromResult(false);

            if (_contentChangePending)
            {
                _contentChangePending = false;

                return Task.FromResult(true);
            }

            _contentChangeWaiter ??= new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            return _contentChangeWaiter.Task;
        }
    }

    /// <summary>
    /// Forwards the drag-start callback to the deriving component when it implements
    /// <see cref="IDragStartListener"/>. Any <see cref="RenderContentAsync"/> the listener performs completes the
    /// JavaScript drag ghost's pending wait, swapping the fresh content into the drag ghost.
    /// </summary>
    [JSInvokable]
    public async Task ForwardDragStartAsync()
    {
        if (_disposed || this is not IDragStartListener listener)
            return;

        await listener.DragStartAsync();
    }

    /// <summary>
    /// Forwards the drag-end callback to the deriving component when it implements
    /// <see cref="IDragEndListener"/>. The drag ghost is being torn down, so nothing is swapped; any render just
    /// refreshes the off-screen content for the next drag.
    /// </summary>
    [JSInvokable]
    public async Task ForwardDragEndAsync()
    {
        if (_disposed || this is not IDragEndListener listener)
            return;

        await listener.DragEndAsync();
    }

    /// <summary>
    /// Forwards the dropzone-enter callback to the deriving component when it implements
    /// <see cref="IDropzoneEnterListener"/>. Any <see cref="RenderContentAsync"/> the listener performs completes
    /// the JavaScript drag ghost's pending wait, swapping the fresh content into the drag ghost.
    /// </summary>
    [JSInvokable]
    public async Task ForwardDropzoneEnterAsync()
    {
        if (_disposed || this is not IDropzoneEnterListener listener)
            return;

        await listener.DropzoneEnterAsync();
    }

    /// <summary>
    /// Forwards the dropzone-leave callback to the deriving component when it implements
    /// <see cref="IDropzoneLeaveListener"/>. Any <see cref="RenderContentAsync"/> the listener performs completes
    /// the JavaScript drag ghost's pending wait, swapping the fresh content into the drag ghost.
    /// </summary>
    [JSInvokable]
    public async Task ForwardDropzoneLeaveAsync()
    {
        if (_disposed || this is not IDropzoneLeaveListener listener)
            return;

        await listener.DropzoneLeaveAsync();
    }

    /// <summary>
    /// Gets a descriptor for the JavaScript module that implements the drag ghost element.
    /// </summary>
    public DragGhostJsModuleDescriptor GetJsModule()
    {
        if (_jsModuleDescriptor is not null)
            return _jsModuleDescriptor;

        if (Content is null)
            throw new InvalidOperationException($"{nameof(DragGhostContent)} not rendered yet or missing in component's render tree.");

        var contentElementReference = Content.GetElementReference();
        if (string.IsNullOrEmpty(contentElementReference.Id))
            throw new InvalidOperationException($"{nameof(DragGhostContent)} did not return a valid element reference.");

        _dotNetObjectReference ??= DotNetObjectReference.Create(this);

        var args = new CreateDragGhostArgs
        {
            DotNetObject = _dotNetObjectReference,
            ContentElementReference = contentElementReference,
            ProcessDragStart = this is IDragStartListener,
            ProcessDragEnd = this is IDragEndListener,
            ProcessDropzoneEnter = this is IDropzoneEnterListener,
            ProcessDropzoneLeave = this is IDropzoneLeaveListener
        };

        _jsModuleDescriptor = new()
        {
            ModuleName = $"/_content/{typeof(DragGhostBase).Assembly.GetName().Name}/draggable/components/drag-ghost-base.js",

            CreateFunction = new()
            {
                Name = "createDragGhost",
                Args = args
            }
        };

        return _jsModuleDescriptor;
    }
}
