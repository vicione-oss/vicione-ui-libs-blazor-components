using Microsoft.AspNetCore.Components;
using Shared.Pages.Grid.Models;
using ViciOne.Ui.Blazor.Components.Grid.Services;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace Shared.Pages.Grid.Components;

[Obsolete("Demonstrates the obsolete Grid")]
public sealed partial class GridPage
{
    private static readonly IList<ExampleGridItem> s_items =
    [
        new ExampleGridItem { Id = Guid.NewGuid(), Key = "Foo", Value = "Bar", Timestamp = DateTime.Now },
        new ExampleGridItem { Id = Guid.NewGuid(), Key = "Lorem ipsum", Value = "Dolor sit amet", Timestamp = DateTime.Now.AddDays(-1) }
    ];

    private bool _virtualize;
    private IQueryable<ExampleGridItem>? _itemsQueryable = s_items.AsQueryable();

    private string? _filterText;
    private bool _filterApplied;
    private bool _hasLeftPane;
    private string _toggleLeftPaneGridActionButtonIconCssClass = string.Empty;

    private ExampleGridItem? _showDetailSource;

    [Inject] private IGridItemSelection<Guid> ItemSelection { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        UpdateToggleLeftPaneGridActionButton();
    }

    private void ApplyFilter()
    {
        _itemsQueryable = s_items.AsQueryable();

        _filterApplied = false;

        if (!string.IsNullOrWhiteSpace(_filterText))
        {
            _itemsQueryable = _itemsQueryable.Where(i => i.Key.Contains(_filterText) || i.Value.Contains(_filterText));

            _filterApplied = true;
        }
    }

    private void ShowDetails(ExampleGridItem item)
        => _showDetailSource = item;

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

    private void AddGridItem()
    {
        s_items.Add(new() { Id = Guid.NewGuid(), Key = "Lorem ipsum", Value = "Dolor sit amet", Timestamp = DateTime.Now.AddDays(-1) });

        _itemsQueryable = s_items.AsQueryable();
    }
}
