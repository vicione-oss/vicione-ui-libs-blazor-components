using ViciOne.Ui.Blazor.Components.ContextMenu.Models;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Services;

/// <summary>
/// Context menu state
/// </summary>
public interface IContextMenuState<TContext> where TContext : IContextMenuContext
{
    /// <summary>
    /// Updates the instance from the given <paramref name="context"/>
    /// </summary>
    void Update(TContext context);
}
