using Bogus;
using Microsoft.AspNetCore.Components;
using Shared.Extensions;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace Shared.Pages.Tables.SimpleTable.Header.Components;

public sealed partial class SimpleTableHeaderPage
{
    private static readonly Faker s_faker = new();
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private IReadOnlyList<ExampleTableItem> _filteredItems = s_items;
    private string? _filterText;
    private bool _filterApplied;
    private bool _hasLeftPane;
    private string _toggleLeftPaneGridActionButtonIconCssClass = string.Empty;
    private ExampleTableItem? _showDetailSource;

    private readonly string _lorem1 = s_faker.Lorem.Paragraph(4);
    private readonly string _lorem2 = s_faker.Lorem.Paragraph(4);

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateToggleLeftPaneGridActionButton();
    }

    private void ApplyFilter(string? filterText)
    {
        _filterText = filterText;
        _filteredItems = [.. s_items];
        _filterApplied = false;

        if (!string.IsNullOrWhiteSpace(_filterText))
        {
            _filteredItems = [.. s_items.Where(item => item.Key.Contains(_filterText, StringComparison.OrdinalIgnoreCase)
                || item.Value.Contains(_filterText, StringComparison.OrdinalIgnoreCase))];
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
        _filteredItems = [.. s_items];
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
