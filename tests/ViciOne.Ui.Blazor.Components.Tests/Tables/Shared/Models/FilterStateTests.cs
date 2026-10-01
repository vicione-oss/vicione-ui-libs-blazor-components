using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

public sealed class FilterTests
{
    [Fact]
    public void Empty_has_no_filters()
    {
        // Arrange & Act
        var filter = FilterState.Empty;

        // Assert
        filter.Filters.Should().BeEmpty();
    }

    [Fact]
    public void With_column_filter_adds_an_entry()
    {
        // Act
        var filter = FilterState.Empty.WithColumnFilter(new TestColumnFilter("A"));

        // Assert
        filter.Filters.Should().ContainSingle()
            .Which.Should().BeOfType<TestColumnFilter>()
            .Which.ColumnId.Should().Be("A");
    }

    [Fact]
    public void With_column_filter_replaces_the_existing_filter_for_the_same_column()
    {
        // Arrange
        var first = new TestColumnFilter("A");
        var second = new TestColumnFilter("A");
        var filter = FilterState.Empty.WithColumnFilter(first);

        // Act
        var updated = filter.WithColumnFilter(second);

        // Assert — one filter per column: the second call replaces, never appends
        updated.Filters.Should().ContainSingle().Which.Should().BeSameAs(second);
    }

    [Fact]
    public void With_column_filter_keeps_distinct_columns()
    {
        // Act
        var filter = FilterState.Empty
            .WithColumnFilter(new TestColumnFilter("A"))
            .WithColumnFilter(new TestColumnFilter("B"));

        // Assert
        filter.Filters.OfType<IColumnFilter>().Select(columnFilter => columnFilter.ColumnId)
            .Should().ContainInOrder("A", "B");
    }

    [Fact]
    public void Without_column_filter_removes_the_entry()
    {
        // Arrange
        var filter = FilterState.Empty
            .WithColumnFilter(new TestColumnFilter("A"))
            .WithColumnFilter(new TestColumnFilter("B"));

        // Act
        var updated = filter.WithoutColumnFilter("A");

        // Assert
        updated.Filters.OfType<IColumnFilter>().Should().ContainSingle().Which.ColumnId.Should().Be("B");
    }

    [Fact]
    public void With_global_filter_adds_an_entry_without_a_column_id()
    {
        // Act
        var filter = FilterState.Empty.WithGlobalFilter(new TestGlobalFilter("a"));

        // Assert — the entry is a global filter, not a column filter
        filter.Filters.Should().ContainSingle().Which.Should().BeOfType<TestGlobalFilter>();
        filter.Filters.OfType<IColumnFilter>().Should().BeEmpty();
    }

    [Fact]
    public void With_global_filter_of_the_same_type_replaces_rather_than_accumulates()
    {
        // Arrange
        var first = new TestGlobalFilter("a");
        var second = new TestGlobalFilter("b");
        var filter = FilterState.Empty.WithGlobalFilter(first);

        // Act
        var updated = filter.WithGlobalFilter(second);

        // Assert — dedup by CLR type: one active global filter of this type, the latest wins
        updated.Filters.Should().ContainSingle().Which.Should().BeSameAs(second);
    }

    [Fact]
    public void With_global_filter_keeps_distinct_types()
    {
        // Act
        var filter = FilterState.Empty
            .WithGlobalFilter(new TestGlobalFilter("a"))
            .WithGlobalFilter(new TestSecondGlobalFilter(1));

        // Assert — two different global-filter types both stay active
        filter.Filters.Should().HaveCount(2);
    }

    [Fact]
    public void Without_global_filter_removes_only_the_named_type()
    {
        // Arrange
        var filter = FilterState.Empty
            .WithGlobalFilter(new TestGlobalFilter("a"))
            .WithGlobalFilter(new TestSecondGlobalFilter(1));

        // Act
        var updated = filter.WithoutGlobalFilter<TestGlobalFilter>();

        // Assert
        updated.Filters.Should().ContainSingle().Which.Should().BeOfType<TestSecondGlobalFilter>();
    }

    [Fact]
    public void Global_and_column_filters_coexist_and_removals_do_not_cross_over()
    {
        // Arrange — one column filter and one global filter together
        var filter = FilterState.Empty
            .WithColumnFilter(new TestColumnFilter("A"))
            .WithGlobalFilter(new TestGlobalFilter("a"));

        // Act — removing the column filter must leave the global filter untouched
        var withoutColumn = filter.WithoutColumnFilter("A");

        // Assert
        withoutColumn.Filters.Should().ContainSingle().Which.Should().BeOfType<TestGlobalFilter>();

        // Act — and removing the global filter must leave the column filter untouched
        var withoutGlobal = filter.WithoutGlobalFilter<TestGlobalFilter>();

        // Assert
        withoutGlobal.Filters.OfType<IColumnFilter>().Should().ContainSingle().Which.ColumnId.Should().Be("A");
    }

    [Fact]
    public void With_global_filter_rejects_a_filter_that_is_also_column_scoped()
    {
        // Arrange — a column filter is keyed by column id; adding it as a global would key it by type and
        // drop every other column's filter of that type. The signature already rejects a plain column filter,
        // so only a type claiming both scopes can reach the guard.
        var filter = FilterState.Empty;

        // Act
        var act = () => filter.WithGlobalFilter(new TestColumnAndGlobalFilter("A"));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*WithColumnFilter*");
    }

    [Fact]
    public void Removing_a_filter_for_an_unfiltered_column_returns_the_same_instance()
    {
        // Arrange — identity is what tells the table "nothing changed", so a no-op must not allocate
        var filter = FilterState.Empty.WithColumnFilter(new TestColumnFilter("A"));

        // Act
        var updated = filter.WithoutColumnFilter("B");

        // Assert
        updated.Should().BeSameAs(filter);
    }

    [Fact]
    public void Setting_the_filter_a_column_already_carries_returns_the_same_instance()
    {
        // Arrange
        var columnFilter = new TestColumnFilter("A");
        var filter = FilterState.Empty.WithColumnFilter(columnFilter);

        // Act
        var updated = filter.WithColumnFilter(columnFilter);

        // Assert
        updated.Should().BeSameAs(filter);
    }

    [Fact]
    public void Removing_the_last_column_filter_returns_the_empty_instance()
    {
        // Arrange — many equal empty values would each read as a change
        var filter = FilterState.Empty.WithColumnFilter(new TestColumnFilter("A"));

        // Act
        var updated = filter.WithoutColumnFilter("A");

        // Assert
        updated.Should().BeSameAs(FilterState.Empty);
    }

    [Fact]
    public void Removing_an_absent_global_filter_returns_the_same_instance()
    {
        // Arrange
        var filter = FilterState.Empty.WithColumnFilter(new TestColumnFilter("A"));

        // Act
        var updated = filter.WithoutGlobalFilter<TestGlobalFilter>();

        // Assert
        updated.Should().BeSameAs(filter);
    }

    [Fact]
    public void Setting_a_column_filter_that_differs_returns_a_new_instance()
    {
        // Arrange
        var filter = FilterState.Empty.WithColumnFilter(new TestColumnFilter("A"));

        // Act
        var updated = filter.WithColumnFilter(new TestColumnFilter("B"));

        // Assert
        updated.Should().NotBeSameAs(filter);
        updated.Filters.Should().HaveCount(2);
    }

    [Fact]
    public void Setting_the_global_filter_already_active_returns_the_same_instance()
    {
        // Arrange — the search-box path re-applies its global filter on every keystroke, so a no-op here is
        // the difference between a provider round-trip per character and none
        var globalFilter = new TestGlobalFilter("a");
        var filter = FilterState.Empty.WithGlobalFilter(globalFilter);

        // Act
        var updated = filter.WithGlobalFilter(globalFilter);

        // Assert
        updated.Should().BeSameAs(filter);
    }

    [Fact]
    public void Setting_a_global_filter_of_the_same_type_that_differs_returns_a_new_instance()
    {
        // Arrange
        var filter = FilterState.Empty.WithGlobalFilter(new TestGlobalFilter("a"));

        // Act
        var updated = filter.WithGlobalFilter(new TestGlobalFilter("b"));

        // Assert
        updated.Should().NotBeSameAs(filter);
        updated.Filters.Should().ContainSingle();
    }

    [Fact]
    public void Removing_the_last_global_filter_returns_the_empty_instance()
    {
        // Arrange — clearing the search box must land on Empty, or the table reads it as a change
        var filter = FilterState.Empty.WithGlobalFilter(new TestGlobalFilter("a"));

        // Act
        var updated = filter.WithoutGlobalFilter<TestGlobalFilter>();

        // Assert
        updated.Should().BeSameAs(FilterState.Empty);
    }
}
