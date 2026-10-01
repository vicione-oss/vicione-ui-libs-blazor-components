using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

/// <summary>
/// A structural component that provides context for a single cell within the <see cref="AdvancedTable{TItem}"/>.
/// It acts as a bridge between the cell and the column it belongs to.
/// </summary>
public sealed partial class CellContainer : IDisposable
{
    /// <summary>
    /// The state of the column this cell belongs to.
    /// </summary>
    [CascadingParameter]
    internal ColumnState ColumnState { get; set; } = default!;

    /// <summary>
    /// The content to be rendered inside this cell container.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
        => ColumnState.RefreshRequested += ColumnStateRefreshRequestedAsync;

    /// <inheritdoc/>
    public void Dispose()
        => ColumnState.RefreshRequested -= ColumnStateRefreshRequestedAsync;

    internal async void ColumnStateRefreshRequestedAsync()
    {
        try
        {
            await InvokeAsync(StateHasChanged);
        }
        catch (ObjectDisposedException)
        {
            // Disposal raced the synchronous event the table raises; this awaits a render and nothing else,
            // so there is no consumer failure to report.
        }
    }
}
