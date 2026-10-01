using Microsoft.AspNetCore.Components;
using Shared.Pages.Tables.Shared.Rows.ContextMenu.Models;
using ViciOne.Ui.Blazor.Components.ContextMenu.Components;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace Shared.Pages.Tables.Shared.Rows.ContextMenu.Components;

public sealed partial class SampleContextMenu : SpecializedContextMenuBase<ExampleRowContextMenuContext>
{
    private static readonly MonochromeIconSize s_iconSize = MonochromeIconSize.Small;

    private readonly string _editIcon = MonochromeIconName.Edit.GetCssClasses(s_iconSize).ToSpaceSeparated();
    private readonly string _detailsIcon = MonochromeIconName.InfoOutlined.GetCssClasses(s_iconSize).ToSpaceSeparated();
    private readonly string _deleteIcon = MonochromeIconName.Delete.GetCssClasses(s_iconSize).ToSpaceSeparated();

    private string HeaderText
        => Context is null ? string.Empty : $"{Context.Items.Count} row(s)";

    /// <summary>
    /// Raised when a menu item is clicked, carrying a label describing the chosen action.
    /// </summary>
    [Parameter]
    public EventCallback<string> ActionInvoked { get; set; }

    private async Task ActionInvokedAsync(string action)
    {
        if (ActionInvoked.HasDelegate)
            await ActionInvoked.InvokeAsync($"{action} — {Context?.Items.Count ?? 0} row(s)");
    }
}
