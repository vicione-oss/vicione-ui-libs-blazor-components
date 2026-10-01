using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using ViciOne.Ui.Blazor.Components.CheckBox;
using ViciOne.Ui.Blazor.Components.Grid.Services;
using QuickGrid = Microsoft.AspNetCore.Components.QuickGrid;

namespace ViciOne.Ui.Blazor.Components.Grid.Components.Columns;

/// <summary>
/// Represents a <see cref="Grid{TGridItem}"/> column whose cells display an item selector.
/// </summary>
/// <typeparam name="TGridItem">Grid item type</typeparam>
/// <typeparam name="TGridItemKey">Grid item key type</typeparam>
[Obsolete(Constants.ObsoleteMessage)]
public sealed class ItemSelectColumn<TGridItem, TGridItemKey> : ColumnBase<TGridItem>, IItemSelectColumn<TGridItem>, IDisposable
{
    private static readonly RenderFragment<QuickGrid.ColumnBase<TGridItem>> s_ownHeaderContent = column => builder =>
    {
        if (column is not ItemSelectColumn<TGridItem, TGridItemKey> itemSelectColumn)
            return;

        var itemCount = itemSelectColumn.GetItemCount();
        var selectedItemCount = itemSelectColumn.Selection.Count;

        bool? value;

        if (itemCount == 0 && selectedItemCount > 0)
            value = null;
        else if (itemCount == 0 && selectedItemCount == 0)
            value = false;
        else if (itemCount == selectedItemCount)
            value = true;
        else if (itemCount > 0 && selectedItemCount > 0)
            value = null;
        else
            value = false;

        var enabled = itemCount > 0;
        var allowIndeterminateState = value is null;
        var valueChangedCallback = EventCallback.Factory.Create<bool?>(itemSelectColumn, itemSelectColumn.HeaderCheckBoxValueChanged);

        builder.OpenComponent<CheckBox<bool?>>(1);
        builder.AddComponentParameter(2, nameof(CheckBox<>.Enabled), enabled);
        builder.AddComponentParameter(3, nameof(CheckBox<>.AllowIndeterminateState), allowIndeterminateState);
        builder.AddComponentParameter(4, nameof(CheckBox<>.Value), value);
        builder.AddComponentParameter(5, nameof(CheckBox<>.ValueChanged), valueChangedCallback);
        builder.CloseComponent();
    };

    private IGridItemSelection<TGridItemKey>? _itemSelection;
    private Expression<Func<TGridItem, TGridItemKey>>? _keySelector;
    private Func<TGridItem, TGridItemKey>? _getItemKey;

    [CascadingParameter]
    private new Grid<TGridItem> Grid { get; set; } = default!;

    /// <summary>
    /// Expression for selecting the key for a <typeparamref name="TGridItem"/>
    /// </summary>
    [Parameter, EditorRequired]
    public required Expression<Func<TGridItem, TGridItemKey>> KeySelector { get; set; }

    /// <summary>
    /// Instance providing currently selected items
    /// </summary>
    [Parameter, EditorRequired]
    public required IGridItemSelection<TGridItemKey> Selection { get; set; }

    /// <summary>
    /// This is only used when Grid.Virtualize is true
    /// </summary>
    [Parameter]
    public int? TotalItemCount { get; set; }

    /// <inheritdoc/>
    public override QuickGrid.GridSort<TGridItem>? SortBy { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        Grid?.ItemSelectColumn = this;
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Class?.Length > 0 && Class != "item-select-column" && !Class.Contains(" item-select-column", StringComparison.Ordinal))
            Class += " item-select-column";
        else
            Class = "item-select-column";

        HeaderTemplate ??= s_ownHeaderContent;

        if (KeySelector != _keySelector)
        {
            _keySelector = KeySelector;

            _getItemKey = KeySelector.Compile();
        }

        if (_itemSelection != Selection)
        {
            _itemSelection?.Changed -= ItemSelectionChangedAsync;

            _itemSelection = Selection;
            _itemSelection.Changed += ItemSelectionChangedAsync;
        }
    }

    /// <inheritdoc/>
    protected override void CellContent(RenderTreeBuilder builder, TGridItem item)
    {
        builder.OpenComponent<CheckBox<bool>>(1);

        builder.AddComponentParameter(2, "Value",
            Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(IsSelected(item)));

        builder.AddComponentParameter(3, "ValueChanged",
            Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(
                EventCallback.Factory.Create<bool>(this, (value) => CellCheckBoxValueChanged(value, item))));

        builder.CloseComponent();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Grid is not null && Grid.ItemSelectColumn == this)
            Grid.ItemSelectColumn = null;

        _itemSelection?.Changed -= ItemSelectionChangedAsync;
    }

    private async void ItemSelectionChangedAsync(GridItemSelectionChangedEventArgs<TGridItemKey> args)
    {
        try
        {
            await base.Grid.RefreshDataAsync();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("InvokeAsync", StringComparison.Ordinal))
        {
            // https://github.com/dotnet/aspnetcore/issues/58794
            await InvokeAsync(StateHasChanged);
        }
    }

    private void HeaderCheckBoxValueChanged(bool? value)
    {
        if (_itemSelection is null)
            return;

        _itemSelection.BeginUpdate();
        try
        {
            _itemSelection.Clear();

            if (value is true)
            {
                var itemKeys = GetKeys();
                if (itemKeys is not null)
                    _itemSelection.AddRange(itemKeys);
            }
        }
        finally
        {
            _itemSelection.EndUpdate();
        }
    }

    private void CellCheckBoxValueChanged(bool value, TGridItem gridItem)
    {
        if (_getItemKey is null)
            return;

        var itemKey = _getItemKey(gridItem);
        if (itemKey is null)
            return;

        if (value)
            _itemSelection?.Add(itemKey);
        else
            _itemSelection?.Remove(itemKey);
    }

    private int? GetItemCount()
    {
        var pagination = base.Grid.Pagination;
        if (pagination is not null)
            return pagination.ItemsPerPage;

        if (Grid.Items is not null)
            return Grid.Items.Count();

        // todo: add support for Grid.ItemsProvider, this will be complicated as we don't have async support in this code path
        /*
        if (Grid.ItemsProvider is not null)
        {
            var request = new GridItemsProviderRequest<TGridItem>
            {
                StartIndex = 0,
                Count = 1 // can we use 0 here?
            };

            var result = await Grid.ItemsProvider.Invoke(request);

            return result.TotalItemCount;
        }
        */

        return null; // running out of options
    }

    private IQueryable<TGridItemKey>? GetKeys()
    {
        if (_getItemKey is null)
            return null;

        if (Grid.Items is not null)
            return Grid.Items.Select(i => _getItemKey(i));

        // todo: add support for Grid.ItemsProvider

        return null; // running out of options
    }

    /// <inheritdoc/>
    public bool IsSelected(TGridItem item)
    {
        if (_getItemKey is null)
            return false;

        var itemKey = _getItemKey(item);

        return Selection.Contains(itemKey);
    }
}
