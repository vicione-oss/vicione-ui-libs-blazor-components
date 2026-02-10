using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace ViciOne.Ui.Blazor.Components.Extensions;

internal static partial class IJSObjectReferenceExtensions
{
    public static async Task DisposeAsync(this IJSObjectReference? jsObjectReference, ILogger logger)
    {
        if (jsObjectReference == null)
            return;

        try
        {
            await jsObjectReference.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
        }
        catch (Exception ex)
        {
            DisposingJsObjectReferenceFailed(logger, ex);
        }
    }

#pragma warning disable CA1068 // CancellationToken parameters must come last
    public static async Task InvokeVoidAsync(this IJSObjectReference? jsObjectReference, string identifier,
        ILogger logger, CancellationToken cancellationToken = default, params object?[]? args)
    {
        if (jsObjectReference == null)
            return;

        try
        {
            await jsObjectReference.InvokeVoidAsync(identifier, cancellationToken, args);
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
            InvokingVoidFailed(logger, ex, identifier, args);
        }
    }
#pragma warning restore CA1068 // CancellationToken parameters must come last

    [LoggerMessage(Level = LogLevel.Error, Message = "Invoking jsObjectReference.{identifier}({args}) failed")]
    private static partial void InvokingVoidFailed(ILogger logger, Exception ex, string identifier, object?[]? args);

    [LoggerMessage(Level = LogLevel.Error, Message = "Disposing jsObjectReference failed")]
    private static partial void DisposingJsObjectReferenceFailed(ILogger logger, Exception ex);
}
