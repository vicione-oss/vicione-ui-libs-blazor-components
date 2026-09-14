using ViciOne.Ui.Blazor.Components.ContextMenu.Models;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Services;

/// <summary>
/// Request for a context menu composed via <see cref="Components.ContextMenu"/>
/// </summary>
public interface IContextMenuRequest<TContext> where TContext : IContextMenuContext
{
    /// <summary>
    /// Raised when <see cref="SendAsync(TContext)"/> was called and
    /// <see cref="IContextMenuSettings.UseCustomMenu"/> is <see langword="true"/>
    /// </summary>
    event Func<TContext, Task>? ContextMenuRequestedAsync;

    /// <summary>
    /// Raises event <see cref="ContextMenuRequestedAsync"/> to notify about the request to display
    /// the context menu associated with the given <paramref name="context"/>, but only when
    /// <see cref="IContextMenuSettings.UseCustomMenu"/> is <see langword="true"/>
    /// </summary>
    Task SendAsync(TContext context);
}
