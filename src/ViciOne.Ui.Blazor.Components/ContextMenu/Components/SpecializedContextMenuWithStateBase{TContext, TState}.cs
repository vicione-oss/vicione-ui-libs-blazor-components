using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;
using ViciOne.Ui.Blazor.Components.ContextMenu.Services;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Components;

/// <summary>
/// Base class with state for components wrapping <see cref="ContextMenu"/>
/// </summary>
public abstract class SpecializedContextMenuWithStateBase<TContext, TState> : SpecializedContextMenuBase<TContext>
    where TContext : class, IContextMenuContext
    where TState : IContextMenuState<TContext>
{
    private TState? _state;

    /// <summary>
    /// State of the context menu wrapped by this component
    /// </summary>
    protected TState State => _state ??= ServiceProvider.GetRequiredKeyedService<TState>(ServiceKey);

    /// <inheritdoc/>
    protected override async Task OnContextMenuRequestedAsync(TContext context)
    {
        State.Update(context);

        await base.OnContextMenuRequestedAsync(context);
    }
}
