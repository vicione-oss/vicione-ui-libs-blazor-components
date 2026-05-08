using Microsoft.AspNetCore.Components;

namespace Shared.Pages.AdvancedErrorBoundary;

public sealed partial class AdvancedErrorBoundaryPage
{
    private readonly string _text = "Make it snap!";
    private string? _recoverMessage;
    private EventCallback<Exception> _prepareRecoverEventCallback;
    private bool _makeItSnap;

    private void UpdatePrepareRecoverEventCallback(bool shouldHavePrepareRecoverEventCallback)
    {
        if (shouldHavePrepareRecoverEventCallback)
            _prepareRecoverEventCallback = EventCallback.Factory.Create<Exception>(this, OnPrepareRecover);
        else
            _prepareRecoverEventCallback = new EventCallback<Exception>();

        _recoverMessage = null;
    }

    private void OnPrepareRecover(Exception error)
    {
        _makeItSnap = false;

        _recoverMessage = $"{nameof(OnPrepareRecover)} called - chance to prepare recover, reset states etc.\n{error.Message}";
    }

    private void MakeItSnapButtonClick()
        => _makeItSnap = true;

    private string? PossiblyFailingOperation()
    {
        if (_makeItSnap)
            throw new InvalidOperationException("😈 A rotten gremlin got us!");

        return null;
    }
}
