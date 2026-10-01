using Microsoft.AspNetCore.Components.Web;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;

namespace Shared.Pages.Tables.Shared.Rows.ContextMenu.Models;

/// <summary>
/// What the row context menu is about — the rows an action applies to, and which entries apply to them.
/// </summary>
public sealed class ExampleRowContextMenuContext : IContextMenuContext
{
    /// <inheritdoc/>
    public ContextMenuItemFilter? ItemFilter { get; init; }

    /// <inheritdoc/>
    public required MouseEventArgs MouseEventArgs { get; init; }

    /// <summary>
    /// The rows the menu acts on: the selection as it stood when the row was right-clicked.
    /// </summary>
    public required IReadOnlyList<ExampleTableItem> Items { get; init; }
}
