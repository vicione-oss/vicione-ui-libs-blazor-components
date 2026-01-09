using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Moveable.Interfaces;

namespace ViciOne.Ui.Blazor.Components.Moveable.Services;

internal sealed partial class MoveInteraction(ILogger<MoveInteraction> logger, IJSRuntime jsRuntime)
    : IMoveInteraction, IAsyncDisposable
{
    private readonly Dictionary<IMoveable, Guid> _moveableIds = [];
    private readonly Dictionary<Guid, IMoveable> _moveables = [];
    private readonly Dictionary<IMoveable, IJSObjectReference> _jsAttachResults = [];
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<MoveInteraction>? _dotNetObjectReference;

    public string StartedCssClass => "moving";
    public string EndedCssClass => "moved";

    public async Task AttachAsync(IMoveable moveable)
    {
        if (!_jsAttachResults.ContainsKey(moveable))
        {
            _jsModule ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import",
                $"./_content/{typeof(MoveInteraction).Assembly.GetName().Name}/moveable/move-interaction.js");

            _dotNetObjectReference ??= DotNetObjectReference.Create(this);

            var moveableId = Guid.NewGuid();
            var moveableElementReference = moveable.GetElementReference();
            var moveHandleElementReference = moveable.GetMoveHandle().GetElementReference();
            var moveContainerElementReference = moveable.GetMoveContainer().GetElementReference();

            var jsAttachResult = await _jsModule.InvokeAsync<IJSObjectReference>("attach",
                moveableId, moveableElementReference, moveHandleElementReference, moveContainerElementReference, StartedCssClass, EndedCssClass,
                _dotNetObjectReference);

            _jsAttachResults.Add(moveable, jsAttachResult);

            _moveableIds.Add(moveable, moveableId);
            _moveables.Add(moveableId, moveable);
        }
    }

    public async Task RemoveAsync(IMoveable moveable)
    {
        await DisposeJsAttachResultAsync(moveable);

        if (_moveableIds.TryGetValue(moveable, out var moveableId))
        {
            _moveables.Remove(moveableId);
            _moveableIds.Remove(moveable);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = $"Invoking jsAttachResult.dispose() failed")]
    public static partial void InvokingJsAttachResultDisposeFailed(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = $"Disposing jsAttachResult failed")]
    public static partial void DisposingJsAttachResultFailed(ILogger logger, Exception ex);

    private async Task DisposeJsAttachResultAsync(IMoveable moveable)
    {
        if (_jsAttachResults.TryGetValue(moveable, out var jsAttachResult))
        {
            try
            {
                await jsAttachResult.InvokeVoidAsync("dispose");
            }
            catch (JSDisconnectedException)
            {
                // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
            }
            catch (Exception ex)
            {
                InvokingJsAttachResultDisposeFailed(logger, ex);
            }

            try
            {
                await jsAttachResult.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
            }
            catch (Exception ex)
            {
                DisposingJsAttachResultFailed(logger, ex);
            }

            _jsAttachResults.Remove(moveable);
        }
    }

    public async ValueTask DisposeAsync()
    {
        var moveables = _jsAttachResults.Keys.ToList();

        foreach (var moveable in moveables)
            await RemoveAsync(moveable);

        _dotNetObjectReference?.Dispose();
        _dotNetObjectReference = null;

        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
            }

            _jsModule = null;
        }
    }

    [JSInvokable]
    public void OnMoveablePointerUp(Guid moveableId, double x, double y)
    {
        if (_moveables.TryGetValue(moveableId, out var moveable))
            moveable.UpdatePosition(x, y);
    }
}
