using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace ViciOne.Ui.Blazor.Components.AdvancedErrorBoundary;

/// <inheritdoc/>
/// <remarks>
/// The content displayed when an exception is captured aligns itself with absolute positioning
/// to the first ancestor element that has position: relative applied.
/// </remarks>
public sealed partial class AdvancedErrorBoundary : ErrorBoundary
{
    [Inject] private ILogger<AdvancedErrorBoundary> Logger { get; set; } = default!;

    /// <summary>
    /// When not <see langword="null"/> a button will be visible to trigger recover manually.
    /// The callback is triggered before recovering from error state.
    /// Passes <see cref="ErrorBoundaryBase.CurrentException"/> that caused the error state.
    /// </summary>
    [Parameter]
    public EventCallback<Exception> PrepareRecover { get; set; }

    /// <inheritdoc/>
    protected override Task OnErrorAsync(Exception exception)
    {
        AnUnexpectedExceptionOccurred(Logger, exception);

        return Task.CompletedTask;
    }

    private async Task TryToRecoverButtonClickAsync()
    {
        try
        {
            await RecoverAsync();
        }
        catch (Exception ex)
        {
            AnUnexpectedExceptionOccurred(Logger, ex);
        }
    }

    /// <inheritdoc cref="ErrorBoundaryBase.Recover"/>
    public async Task RecoverAsync()
    {
        await PrepareRecover.InvokeAsync(CurrentException);

        base.Recover();
    }

    /// <inheritdoc cref="ErrorBoundaryBase.Recover"/>
    [Obsolete("Call " + nameof(RecoverAsync) + " instead")]
    public new void Recover()
        => base.Recover();

    [LoggerMessage(Level = LogLevel.Error, Message = "An unexpected exception occurred.")]
    private static partial void AnUnexpectedExceptionOccurred(ILogger logger, Exception ex);
}
