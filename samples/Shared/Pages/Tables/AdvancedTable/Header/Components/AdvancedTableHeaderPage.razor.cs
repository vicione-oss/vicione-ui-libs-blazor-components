using Bogus;
using Microsoft.AspNetCore.Components;
using Shared.Extensions;
using Shared.Pages.Tables.AdvancedTable.Services;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace Shared.Pages.Tables.AdvancedTable.Header.Components;

public sealed partial class AdvancedTableHeaderPage
{
    private static readonly Faker s_faker = new();
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private IQueryable<ExampleTableItem> _itemsQueryable = s_items.AsQueryable();
    private string? _filterText;
    private bool _filterApplied;
    private bool _hasLeftPane;
    private string _toggleLeftPaneGridActionButtonIconCssClass = string.Empty;
    private ExampleTableItem? _showDetailSource;

    private readonly string _lorem1 = s_faker.Lorem.Paragraph(4);
    private readonly string _lorem2 = s_faker.Lorem.Paragraph(4);

    private IItemsProvider<ExampleTableItem> FilteredItemsProvider => new ExampleTableItemsProvider(_itemsQueryable);

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateToggleLeftPaneGridActionButton();
    }

    private void ApplyFilter(string? filterText)
    {
        _filterText = filterText;
        _itemsQueryable = s_items.AsQueryable();
        _filterApplied = false;

        if (!string.IsNullOrWhiteSpace(_filterText))
        {
            _itemsQueryable = _itemsQueryable.Where(item => item.Key.Contains(_filterText) || item.Value.Contains(_filterText));
            _filterApplied = true;
        }
    }

    private void ShowDetails(ExampleTableItem item) => _showDetailSource = item;

    private void ToggleLeftPane()
    {
        _hasLeftPane = !_hasLeftPane;
        UpdateToggleLeftPaneGridActionButton();
    }

    private void UpdateToggleLeftPaneGridActionButton()
    {
        var iconName = _hasLeftPane ? MonochromeIconName.Open : MonochromeIconName.Folder;
        _toggleLeftPaneGridActionButtonIconCssClass = iconName.GetCssClasses(MonochromeIconSize.SmallMedium).ToSpaceSeparated();
    }

    private async Task AddItemAsync()
    {
        s_items.Add(ExampleTableItemFactory.Create());
        _itemsQueryable = s_items.AsQueryable();
        await InvokeAsync(StateHasChanged);
    }

    private MarkupString GetCellContent(string value)
    {
        var result = new MarkupString(value);
        if (_filterApplied)
            result = result.WithHighlightedText(_filterText);
        return result;
    }
}
