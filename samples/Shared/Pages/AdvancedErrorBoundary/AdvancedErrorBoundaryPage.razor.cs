using Microsoft.AspNetCore.Components;

namespace Shared.Pages.AdvancedErrorBoundary;

public sealed partial class AdvancedErrorBoundaryPage
{
    private string? _recoverMessage;
    private EventCallback<Exception> _prepareRecoverEventCallback;

    private void UpdatePrepareRecoverEventCallback(bool shouldHavePrepareRecoverEventCallback)
    {
        if (shouldHavePrepareRecoverEventCallback)
            _prepareRecoverEventCallback = EventCallback.Factory.Create<Exception>(this, OnPrepareRecover);
        else
            _prepareRecoverEventCallback = new EventCallback<Exception>();

        _recoverMessage = null;
    }

    private void OnPrepareRecover(Exception error)
        => _recoverMessage = $"{nameof(OnPrepareRecover)} called - chance to prepare recover, reset states etc.\n{error.Message}";

    private static void MakeItSnapButtonClick()
        => throw new InvalidOperationException("😈 A rotten gremlin got us!");
}
