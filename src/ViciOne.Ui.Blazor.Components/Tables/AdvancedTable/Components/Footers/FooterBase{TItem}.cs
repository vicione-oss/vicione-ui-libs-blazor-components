using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Footers;

/// <summary>
/// Base class for <see cref="AdvancedTable{TItem}"/> footers.
/// </summary>
public abstract class FooterBase<TItem> : ComponentBase, IDisposable
    where TItem : class
{
    private int _computedItemCount;
    private int? _previousItemCount;
    private bool _hasPreviousItemCount;

    /// <summary>
    /// The parent <see cref="IAdvancedTable{TItem}"/> instance.
    /// </summary>
    [CascadingParameter]
    internal IAdvancedTable<TItem> Table { get; set; } = default!;

    /// <summary>
    /// The number of items to display. Left unset, the number of items the table currently holds is displayed
    /// and kept current as the table reloads.
    /// </summary>
    /// <remarks>
    /// <c>0</c> is an override like any other number; only <c>null</c> falls back to the table's own count.
    /// </remarks>
    [Parameter]
    public int? ItemCount { get; set; }

    /// <summary>
    /// The number of items in force: <see cref="ItemCount"/> where one is supplied, otherwise the number of
    /// items the table reports.
    /// </summary>
    private protected int EffectiveItemCount => ItemCount ?? _computedItemCount;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        if (Table is null)
        {
            var message = $"{GetType().FullName} must be placed inside a {typeof(AdvancedTable<TItem>).FullName}.";
            throw new InvalidOperationException(message);
        }

        Table.ItemsChanged += TableItemsChanged;
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (_hasPreviousItemCount && ItemCount == _previousItemCount)
            return;

        _previousItemCount = ItemCount;
        _hasPreviousItemCount = true;

        if (ItemCount is null)
            _computedItemCount = Table.GetTotalItemCount();
    }

    private async void TableItemsChanged()
    {
        // A supplied count wins, so there is nothing to read from the table and nothing to re-render for.
        if (ItemCount is not null)
            return;

        _computedItemCount = Table.GetTotalItemCount();

        try
        {
            await InvokeAsync(StateHasChanged);
        }
        catch (ObjectDisposedException)
        {
            // Disposal raced the synchronous event the table raises; this awaits a render and nothing else.
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs synchronous clean-up
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && Table is not null)
            Table.ItemsChanged -= TableItemsChanged;
    }
}
