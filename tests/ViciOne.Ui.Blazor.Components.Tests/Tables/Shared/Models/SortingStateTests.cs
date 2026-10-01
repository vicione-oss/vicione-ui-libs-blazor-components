using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

public sealed class SortingTests
{
    [Fact]
    public void Empty_has_no_column_sortings()
    {
        // Arrange & Act
        var sorting = SortingState.Empty;

        // Assert
        sorting.ColumnSortings.Should().BeEmpty();
    }

    [Fact]
    public void With_column_sorting_adds_an_entry()
    {
        // Act
        var sorting = SortingState.Empty.WithColumnSorting(new ColumnSorting("A", Ascending: true));

        // Assert
        sorting.ColumnSortings.Should().ContainSingle().Which.Should().Be(new ColumnSorting("A", true));
    }

    [Fact]
    public void With_column_sorting_overload_sets_the_direction()
    {
        // Act
        var sorting = SortingState.Empty.WithColumnSorting("A", ascending: false);

        // Assert
        sorting.ColumnSortings.Should().ContainSingle().Which.Should().Be(new ColumnSorting("A", false));
    }

    [Fact]
    public void With_column_sorting_replaces_the_existing_entry_for_the_same_column()
    {
        // Arrange
        var sorting = SortingState.Empty.WithColumnSorting("A", ascending: true);

        // Act
        var updated = sorting.WithColumnSorting("A", ascending: false);

        // Assert — one entry per column: the second call replaces, never appends
        updated.ColumnSortings.Should().ContainSingle().Which.Should().Be(new ColumnSorting("A", false));
    }

    [Fact]
    public void With_column_sorting_keeps_distinct_columns()
    {
        // Act
        var sorting = SortingState.Empty
            .WithColumnSorting("A", ascending: true)
            .WithColumnSorting("B", ascending: true);

        // Assert
        sorting.ColumnSortings.Select(columnSorting => columnSorting.ColumnId).Should().ContainInOrder("A", "B");
    }

    [Fact]
    public void Without_column_sorting_removes_the_entry()
    {
        // Arrange
        var sorting = SortingState.Empty.WithColumnSorting("A", ascending: true).WithColumnSorting("B", ascending: true);

        // Act
        var updated = sorting.WithoutColumnSorting("A");

        // Assert
        updated.ColumnSortings.Should().ContainSingle().Which.ColumnId.Should().Be("B");
    }

    [Fact]
    public void With_column_sorting_keeps_the_position_of_a_column_it_replaces()
    {
        // Arrange — the list order is the sort priority, so re-sorting the primary column must not demote it
        var sorting = SortingState.Empty
            .WithColumnSorting("A", ascending: true)
            .WithColumnSorting("B", ascending: true);

        // Act
        var updated = sorting.WithColumnSorting("A", ascending: false);

        // Assert
        updated.ColumnSortings.Should().Equal(new ColumnSorting("A", false), new ColumnSorting("B", true));
    }

    [Fact]
    public void With_column_sorting_appends_a_column_that_is_not_yet_sorted()
    {
        // Arrange
        var sorting = SortingState.Empty.WithColumnSorting("A", ascending: true);

        // Act
        var updated = sorting.WithColumnSorting("B", ascending: true);

        // Assert — a new column joins as the lowest priority
        updated.ColumnSortings.Select(columnSorting => columnSorting.ColumnId).Should().Equal("A", "B");
    }

    [Fact]
    public void Copy_methods_do_not_mutate_the_source_instance()
    {
        // Arrange
        var sorting = SortingState.Empty.WithColumnSorting("A", ascending: true);

        // Act
        _ = sorting.WithColumnSorting("B", ascending: true);

        // Assert
        sorting.ColumnSortings.Should().ContainSingle().Which.ColumnId.Should().Be("A");
    }

    [Fact]
    public void Removing_the_sorting_of_an_unsorted_column_returns_the_same_instance()
    {
        // Arrange — identity is what tells the table "nothing changed", so a no-op must not allocate
        var sorting = SortingState.Empty.WithColumnSorting("A", ascending: true);

        // Act
        var updated = sorting.WithoutColumnSorting("B");

        // Assert
        updated.Should().BeSameAs(sorting);
    }

    [Fact]
    public void Setting_the_sorting_a_column_already_has_returns_the_same_instance()
    {
        // Arrange
        var sorting = SortingState.Empty.WithColumnSorting("A", ascending: true);

        // Act
        var updated = sorting.WithColumnSorting("A", ascending: true);

        // Assert
        updated.Should().BeSameAs(sorting);
    }

    [Fact]
    public void Removing_the_last_column_sorting_returns_the_empty_instance()
    {
        // Arrange — many equal empty values would each read as a change
        var sorting = SortingState.Empty.WithColumnSorting("A", ascending: true);

        // Act
        var updated = sorting.WithoutColumnSorting("A");

        // Assert
        updated.Should().BeSameAs(SortingState.Empty);
    }

    [Fact]
    public void Reversing_the_direction_of_a_sorted_column_returns_a_new_instance()
    {
        // Arrange
        var sorting = SortingState.Empty.WithColumnSorting("A", ascending: true);

        // Act
        var updated = sorting.WithColumnSorting("A", ascending: false);

        // Assert
        updated.Should().NotBeSameAs(sorting);
        updated.ColumnSortings.Should().ContainSingle().Which.Ascending.Should().BeFalse();
    }
}
