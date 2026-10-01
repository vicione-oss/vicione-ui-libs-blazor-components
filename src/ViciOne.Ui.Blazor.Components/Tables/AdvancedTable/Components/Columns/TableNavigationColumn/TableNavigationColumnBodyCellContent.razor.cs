using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns.TableNavigationColumn;

/// <summary>
/// Body cell content of a <see cref="TableNavigationColumn{TItem}"/>.
/// </summary>
public sealed partial class TableNavigationColumnBodyCellContent<TItem> : TableBodyCellContentBase<TItem>, IDisposable
    where TItem : class
{
    /// <summary>
    /// The item associated with the current row.
    /// </summary>
    [Parameter, EditorRequired]
    public required TItem Item { get; set; }

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#attr-title">title</see> attribute of the navigation buttons
    /// </summary>
    [Parameter]
    public string? Tooltip { get; set; }

    /// <summary>
    /// Raised when a navigation button has been clicked
    /// </summary>
    [Parameter]
    public EventCallback<TItem> Navigate { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        RowState.HoveredChanged += RowStateHoveredChangedAsync;
    }

    /// <inheritdoc/>
    public void Dispose()
        => RowState.HoveredChanged -= RowStateHoveredChangedAsync;

    private async void RowStateHoveredChangedAsync()
    {
        try
        {
            await InvokeAsync(StateHasChanged);
        }
        catch (ObjectDisposedException)
        {
            // Disposal raced the synchronous event the row raises; this awaits a render and nothing else.
        }
    }

    private async Task NavigateButtonClickAsync()
    {
        if (Navigate.HasDelegate)
            await Navigate.InvokeAsync(Item);
    }
}
