using Microsoft.JSInterop;

namespace ViciOne.Ui.Blazor.Components.Tests.Extensions;

/// <summary>
/// Stands in for an imported JavaScript module whose constructors fail, e.g. because an element passed to them
/// is no longer in the DOM. bUnit cannot set up a failure for invocations returning <see cref="IJSObjectReference"/>.
/// </summary>
internal sealed class ConstructorThrowingJSObjectReference(Exception exception) : IJSObjectReference
{
    public List<(string Identifier, CancellationToken CancellationToken, object?[]? Args)> ConstructorInvocations { get; } = [];

    public ValueTask<IJSObjectReference> InvokeConstructorAsync(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        ConstructorInvocations.Add((identifier, cancellationToken, args));

        return ValueTask.FromException<IJSObjectReference>(exception);
    }

    public ValueTask<IJSObjectReference> InvokeConstructorAsync(string identifier, object?[]? args)
        => InvokeConstructorAsync(identifier, CancellationToken.None, args);

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        => ValueTask.FromResult<TValue>(default!);

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        => ValueTask.FromResult<TValue>(default!);

    public ValueTask DisposeAsync()
        => ValueTask.CompletedTask;
}
