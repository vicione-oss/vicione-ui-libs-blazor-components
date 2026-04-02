using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;
using ViciOne.Ui.Blazor.Components.ContextMenu.Services;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Components;

/// <summary>
/// Base class for components wrapping <see cref="Components.ContextMenu"/>.
/// </summary>
/// <typeparam name="TContext">
/// The context type that describes the item and mouse event used to show the menu.
/// Must implement <see cref="IContextMenuContext"/>.
/// </typeparam>
public abstract class SpecializedContextMenuBase<TContext> : ComponentBase, IDisposable
    where TContext : class, IContextMenuContext
{
    private bool _disposed;
    private IContextMenuRequest<TContext>? _request;

    /// <summary>
    /// Service provider used to resolve context menu services at runtime.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="IServiceProvider"/> instead of keyed DI injection due to
    /// <see href="https://github.com/dotnet/roslyn/issues/67569">roslyn#67569</see>.
    /// </remarks>
    [Inject] protected IServiceProvider ServiceProvider { get; set; } = default!;

    /// <summary>
    /// Optional service key used to resolve context menu services.
    /// </summary>
    [Parameter] public object? ServiceKey { get; set; }

    /// <summary>
    /// The current context retrieved from <see cref="IContextMenuRequest{TContext}"/>.
    /// </summary>
    protected TContext? Context { get; set; }

    /// <summary>
    /// Reference to the component that renders the actual context menu.
    /// </summary>
    protected ContextMenu? ContextMenu { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases managed resources and detaches event handlers.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release managed resources; otherwise <see langword="false"/>.
    /// </param>
    /// <remarks>
    /// Unsubscribes from <see cref="IContextMenuRequest{TContext}.ContextMenuRequestedAsync"/> when disposing.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing && _request is not null)
            _request.ContextMenuRequestedAsync -= OnContextMenuRequestedAsync;

        _disposed = true;
    }

    /// <summary>
    /// Invoked when <see cref="IContextMenuRequest{TContext}.ContextMenuRequestedAsync"/> is raised.
    /// Shows the context menu if a <see cref="ContextMenu"/> reference is available.
    /// </summary>
    /// <param name="context">The request context (item and mouse event).</param>
    protected virtual async Task OnContextMenuRequestedAsync(TContext context)
    {
        Context = context;

        if (ContextMenu is not null)
            await ContextMenu.ShowAsync(context.MouseEventArgs, context.ItemFilter);
    }

    /// <summary>
    /// Resolves the request service and subscribes to context menu requests.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="ServiceProvider"/> to obtain <see cref="IContextMenuRequest{TContext}"/>,
    /// optionally keyed by <see cref="ServiceKey"/>, and registers
    /// <see cref="OnContextMenuRequestedAsync(TContext)"/> as the event handler.
    /// </remarks>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        _request = ServiceProvider.GetRequiredKeyedService<IContextMenuRequest<TContext>>(ServiceKey);
        _request.ContextMenuRequestedAsync += OnContextMenuRequestedAsync;
    }
}
