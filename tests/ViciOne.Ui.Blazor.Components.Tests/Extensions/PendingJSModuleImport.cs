using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace ViciOne.Ui.Blazor.Components.Tests.Extensions;

/// <summary>
/// Holds the import of a JavaScript module pending until <see cref="CompleteAsync"/> is called, so a test can dispose
/// the importing component while its module is still loading. Every other call goes to bUnit's JS runtime.
/// bUnit cannot set up a pending invocation returning <see cref="IJSObjectReference"/>.
/// </summary>
internal sealed class PendingJSModuleImport(IJSRuntime jsRuntime, string modulePath) : IJSRuntime
{
    private static readonly TimeSpan s_timeout = TimeSpan.FromSeconds(5);

    private readonly TaskCompletionSource<IJSObjectReference> _import =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private volatile bool _requested;

    /// <summary>
    /// The module the import resolves to.
    /// </summary>
    public IJSObjectReference Module { get; } = Substitute.For<IJSObjectReference>();

    /// <summary>
    /// Registers a <see cref="PendingJSModuleImport"/> as the JS runtime of <paramref name="testContext"/>.
    /// </summary>
    public static PendingJSModuleImport SetUp(BunitContext testContext, string modulePath)
    {
        var pendingImport = new PendingJSModuleImport(testContext.JSInterop.JSRuntime, modulePath);

        testContext.Services.AddSingleton<IJSRuntime>(pendingImport);

        return pendingImport;
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        if (identifier != "import" || args is not [string path, ..] || path != modulePath)
            return jsRuntime.InvokeAsync<TValue>(identifier, cancellationToken, args);

        _requested = true;

        return new ValueTask<TValue>((Task<TValue>)(object)_import.Task);
    }

    /// <summary>
    /// Waits up to five seconds for the import to be requested, for components that import outside a render.
    /// </summary>
    public async Task WaitUntilRequestedAsync()
    {
        await WaitUntilAsync(() => _requested);

        _requested.Should().BeTrue("the module has to be loading when the component is disposed");
    }

    /// <summary>
    /// Resolves the import and waits up to five seconds for the module to be disposed.
    /// </summary>
    public async Task CompleteAsync()
    {
        _import.SetResult(Module);

        await WaitUntilAsync(() => Module.ReceivedCalls()
            .Any(c => c.GetMethodInfo().Name == nameof(IAsyncDisposable.DisposeAsync)));
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(s_timeout);

        while (!condition() && !timeout.IsCancellationRequested)
            await Task.Delay(10, CancellationToken.None);
    }
}
