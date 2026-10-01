using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Filters;

/// <summary>
/// Renders the filter icon in a column header and owns whether the column's filter panel is open.
/// Creates a <see cref="ColumnFilterEditorContext"/> and cascades it over the projected
/// <see cref="FilterEditor"/>, which <see cref="ColumnFilterPanel"/> hosts. Raises
/// <see cref="OpenChanged"/> so the table can suppress column-drag on the owning header while the panel
/// is open.
/// </summary>
public sealed partial class ColumnFilterButton : ComponentBase
{
    private bool _open;
    private ElementReference _buttonElement;
    private ColumnFilterEditorContext? _panelContext;

    /// <summary>
    /// The id of the column this trigger belongs to.
    /// </summary>
    [Parameter, EditorRequired]
    public required string ColumnId { get; set; }

    /// <summary>
    /// The filter editor, projected by the column via <c>&lt;FilterEditor&gt;</c> into the panel.
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment FilterEditor { get; set; }

    /// <summary>
    /// The current active filter for this column, or <see langword="null"/> if no filter is active.
    /// </summary>
    [Parameter]
    public IColumnFilter? Filter { get; set; }

    /// <summary>
    /// Raised when the editor applies a filter to this one column, keyed by <see cref="ColumnId"/>. Passes
    /// <see langword="null"/> to clear that column's filter. Carries a single column's filter, never the
    /// table's whole filter state.
    /// </summary>
    [Parameter]
    public EventCallback<IColumnFilter?> ColumnFilterChanged { get; set; }

    /// <summary>
    /// Raised when the panel opens (<see langword="true"/>) or closes (<see langword="false"/>).
    /// </summary>
    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    private async Task ToggleAsync()
    {
        if (_open)
        {
            await CloseAsync();

            return;
        }

        _open = true;

        // The context is a snapshot taken as the panel opens. The editor reads the filter as it stood at
        // that moment, so a filter changed from elsewhere on the page while the panel is open never
        // rewrites what the user is in the middle of typing.
        _panelContext = new(ColumnId, Filter, ColumnFilterChanged,
            EventCallback.Factory.Create(this, CloseAsync));

        await NotifyOpenChangedAsync();

        StateHasChanged();
    }

    private async Task CloseAsync()
    {
        _open = false;
        _panelContext = null;

        await NotifyOpenChangedAsync();

        StateHasChanged();
    }

    private async Task NotifyOpenChangedAsync()
    {
        if (OpenChanged.HasDelegate)
            await OpenChanged.InvokeAsync(_open);
    }
}
