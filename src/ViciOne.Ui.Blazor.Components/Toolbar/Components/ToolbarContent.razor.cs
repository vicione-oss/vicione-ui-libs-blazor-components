using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Toolbar.Components;

/// <summary>
/// Container for arbitrary content inside a <see cref="Toolbar"/> by allowing content projection.
/// </summary>
public sealed partial class ToolbarContent : ToolbarItemBase, IToolbarContent
{
    private ElementReference _container;

    /// <inheritdoc/>
    protected override ElementReference Container => _container;

    /// <summary>
    /// Content to be rendered inside the toolbar item.
    /// </summary>
    [Parameter]
    public RenderFragment<IToolbarContent>? ChildContent { get; set; }

    /// <inheritdoc/>
    public bool IsInMenu()
        => InMenu;
}
