using System.Globalization;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Models;

public sealed class SimpleTableContainsColumnFilterTests
{
    [Theory]
    [InlineData("Banana", "ban", true)]
    [InlineData("Banana", "ANA", true)]
    [InlineData("Banana", "xyz", false)]
    public void Matches_is_case_insensitive_substring(string value, string filterValue, bool expected)
    {
        // Arrange
        var filter = new SimpleTableContainsColumnFilter<TableTestItem>("Name", filterValue,
            item => item.TestValue);

        // Act
        var matches = filter.Matches(new TableTestItem(1, value));

        // Assert
        matches.Should().Be(expected);
    }

    [Fact]
    public void Matches_returns_false_when_projection_is_null()
    {
        // Arrange
        var filter = new SimpleTableContainsColumnFilter<TableTestItem>("Name", "anything", _ => null);

        // Act
        var matches = filter.Matches(new TableTestItem(1, "Banana"));

        // Assert
        matches.Should().BeFalse();
    }

    [Fact]
    public void Matches_works_on_non_string_members_via_a_stringifying_selector()
    {
        // Arrange
        var filter = new SimpleTableContainsColumnFilter<TableTestItem>(
            "Key", "3", item => item.TestKey.ToString(CultureInfo.InvariantCulture));

        // Act
        var matchesKey30 = filter.Matches(new TableTestItem(30, "x"));
        var matchesKey1 = filter.Matches(new TableTestItem(1, "x"));

        // Assert
        matchesKey30.Should().BeTrue();
        matchesKey1.Should().BeFalse();
    }

    [Fact]
    public void Public_constructor_carries_column_id_so_the_filter_can_be_seeded_in_code()
    {
        // Arrange & Act
        var filter = new SimpleTableContainsColumnFilter<TableTestItem>("Value", "b", item => item.TestValue);

        // Assert
        filter.ColumnId.Should().Be("Value");
        filter.Value.Should().Be("b");
    }
}
