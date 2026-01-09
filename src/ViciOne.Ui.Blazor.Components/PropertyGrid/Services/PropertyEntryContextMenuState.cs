using ViciOne.Ui.Blazor.Components.ContextMenu.Services;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

/// <inheritdoc cref="IContextMenuState{TContext}"/>
public sealed class PropertyEntryContextMenuState : IContextMenuState<PropertyEntryContextMenuContext>
{
    /// <summary>
    /// Header text displayed in the context menu.
    /// </summary>
    public string HeaderText { get; private set; } = string.Empty;

    /// <summary>
    /// <see langword="true"/> when the "Reset" option is visible, otherwise <see langword="false"/>.
    /// </summary>
    public bool ResetVisible { get; private set; }

    /// <summary>
    /// <see langword="true"/> when the "Set to null" option is visible, otherwise <see langword="false"/>.
    /// </summary>
    public bool SetToNullVisible { get; private set; }

    /// <inheritdoc/>
    public void Update(PropertyEntryContextMenuContext context)
    {
        HeaderText = context.PropertyGridItem.DisplayName;
        ResetVisible = context.PropertyGridItem.Resettable;
        SetToNullVisible = context.PropertyGridItem.CanBeSetToNull;
    }
}
