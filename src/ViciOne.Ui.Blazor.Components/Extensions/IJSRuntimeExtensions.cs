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

    public static async Task<IJSObjectReference?> ImportModuleAsync(this IJSRuntime jsRuntime, string modulePath, ILogger logger)
    {
        try
        {
            // Takes no cancellation token: a canceled import still loads the module and creates its object reference in
            // the browser, which then never reaches .NET and cannot be released.
            return await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);
        }
        catch (JSDisconnectedException)
        {
            // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
        }
        catch (OperationCanceledException)
        {
            // The JavaScript interop call timed out, return gracefully
        }
        catch (Exception ex)
        {
            ImportingFailed(logger, ex, modulePath);
        }

        return null;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Importing module {modulePath} failed")]
    private static partial void ImportingFailed(ILogger logger, Exception ex, string modulePath);
}
