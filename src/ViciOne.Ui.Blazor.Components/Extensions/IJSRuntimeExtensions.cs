using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace ViciOne.Ui.Blazor.Components.Extensions;

internal static partial class IJSRuntimeExtensions
{
    public static async Task ImportAsync(this IJSRuntime jsRuntime, string modulePath, ILogger logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("import", cancellationToken, modulePath);
        }
        catch (JSDisconnectedException)
        {
            // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
        }
        catch (OperationCanceledException)
        {
            // Operation canceled, return gracefully
        }
        catch (Exception ex)
        {
            ImportingFailed(logger, ex, modulePath);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Importing module {modulePath} failed")]
    private static partial void ImportingFailed(ILogger logger, Exception ex, string modulePath);
}
