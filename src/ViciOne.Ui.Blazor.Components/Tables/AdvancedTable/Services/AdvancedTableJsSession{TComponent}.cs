using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Services;

/// <inheritdoc cref="IAdvancedTableJsSession"/>
/// <typeparam name="TComponent">
/// The component JavaScript calls back into. Named as a type parameter rather than taken as
/// <see cref="object"/> so the .NET object reference handed over keeps the component's own type, which is what
/// its JSInvokable methods are resolved against.
/// </typeparam>
internal sealed class AdvancedTableJsSession<TComponent>(IJSRuntime jsRuntime, TComponent component)
    : IAdvancedTableJsSession
        where TComponent : class
{
    // The import is deferred until something first asks for the module, so a table that never renders never
    // pays for it.
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() => jsRuntime
        .InvokeAsync<IJSObjectReference>("import",
            $"./_content/{typeof(AdvancedTableJsSession<TComponent>).Assembly.GetName().Name}/advanced-table/components/advanced-table.js")
        .AsTask());

    private IJSObjectReference? _attachResult;

    // Held only so it can be disposed with the session: JavaScript keeps its own handle on it for as long as
    // the attachment lasts.
    private DotNetObjectReference<TComponent>? _componentReference;

    private bool _attaching;
    private bool _disposed;

    /// <inheritdoc/>
    public bool IsAttached => _attachResult is not null;

    /// <inheritdoc/>
    public Task<IJSObjectReference> GetModuleAsync()
        => _moduleTask.Value;

    /// <inheritdoc/>
    public async Task AttachAsync(ElementReference tableElement)
    {
        // The flag is claimed atomically because attaching spans two awaits: a render arriving in between
        // would otherwise start a second attachment and leak the first one's handle.
        if (_attachResult is not null || _disposed || Interlocked.CompareExchange(ref _attaching, true, false))
            return;

        try
        {
            var module = await _moduleTask.Value;

            _componentReference = DotNetObjectReference.Create(component);

            _attachResult = await module.InvokeAsync<IJSObjectReference>("attach", tableElement, _componentReference);
        }
        finally
        {
            _attaching = false;
        }
    }

    /// <inheritdoc/>
    public Task UpdatePinnedOffsetsAsync()
        => InvokeAttachedAsync("updatePinnedOffsets");

    /// <inheritdoc/>
    public Task ResetFocusedCellAsync()
        => InvokeAttachedAsync("resetFocusedCell");

    /// <inheritdoc/>
    public Task EnterFocusedCellAsync()
        => InvokeAttachedAsync("enterFocusedCell");

    /// <inheritdoc/>
    public Task ColumnsChangedAsync()
        => InvokeAttachedAsync("columnsChanged");

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposed, true, false))
            return;

        try
        {
            if (_attachResult is not null)
            {
                await _attachResult.InvokeVoidAsync("dispose");
                await _attachResult.DisposeAsync();
            }

            if (_moduleTask.IsValueCreated && _moduleTask.Value.IsCompletedSuccessfully)
            {
                var module = await _moduleTask.Value;

                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // Circuit is gone; nothing to resolve against anymore.
        }
        finally
        {
            _componentReference?.Dispose();
        }
    }

    // The one place a call into the attached module is made, so the two ways such a call can legitimately fail
    // are answered once instead of at every call site.
    // IJSObjectReferenceExtensions.InvokeVoidAsync is deliberately not used here: it reports
    // ObjectDisposedException as an error, while a disposal that completed mid-call is expected and says
    // nothing worth logging.
    private async Task InvokeAttachedAsync(string identifier)
    {
        if (_attachResult is null || _disposed)
            return;

        try
        {
            await _attachResult.InvokeVoidAsync(identifier);
        }
        catch (JSDisconnectedException)
        {
            // Circuit is gone; nothing to call against anymore.
        }
        catch (ObjectDisposedException)
        {
            // Disposal raced the call; nothing to call against anymore.
        }
    }
}
