using System.Globalization;
using Bunit;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

public sealed partial class AdvancedTableTests
{
    [Fact]
    public void Striped_gives_every_other_row_the_alternate_css_class()
    {
        // Arrange & Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(BuildItems(4)))
            .Add(p => p.Striped, true));

        // Assert
        rendered.FindAll("tbody tr").Select(row => row.ClassList.Contains("alternate")).Should()
            .Equal(true, false, true, false);
    }

    [Fact]
    public void Rows_have_no_alternate_css_class_without_striped()
    {
        // Arrange & Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(BuildItems(4))));

        // Assert
        rendered.FindAll("tbody tr.alternate").Should().BeEmpty();
    }

    [Fact]
    public async Task Striping_follows_the_absolute_row_index_once_the_loaded_window_has_moved()
    {
        // Arrange
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new RangedItemsProvider(BuildItems(60)))
            .Add(p => p.Mode, TableLoadingMode.Virtualize)
            .Add(p => p.ItemIdSelector, item => item.TestKey)
            .Add(p => p.Striped, true)
            .Add(p => p.Columns, KeyboardColumns()));

        // Act — a window starting at an odd row is where striping by rendered position would color the rows
        // the other way around
        await MoveLoadedWindowAsync(rendered, startIndex: 51, count: 4);

        // Assert
        var movedWindowRows = rendered.FindAll("tbody tr")
            .Where(row => int.Parse(row.QuerySelector("td")!.GetAttribute("data-row-index")!, CultureInfo.InvariantCulture) >= 51)
            .ToList();

        movedWindowRows.Should().HaveCount(4);
        movedWindowRows.Select(row => row.ClassList.Contains("alternate")).Should().Equal(false, true, false, true);
    }
}
