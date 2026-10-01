using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Services;

public sealed class SimpleTableItemsProviderTests
{
    private static readonly ILogger<SimpleTableItemsProvider<TableTestItem>> s_nullLogger =
        Substitute.For<ILogger<SimpleTableItemsProvider<TableTestItem>>>();

    private static readonly ItemsProviderContext s_noSortingNoRange = new();

    // A fresh resolver per provider, mirroring how DI hands SimpleTable<TItem> a resolver per table: reusing
    // one across tests would leak its per-column comparer cache and misconfiguration-log de-dup between them.
    private static SimpleTableItemsProvider<TableTestItem> CreateProvider(
        IReadOnlyList<TableTestItem> items,
        Func<IReadOnlyCollection<IAdvancedTableColumn<TableTestItem>>> columns,
        ILogger<SimpleTableItemsProvider<TableTestItem>>? logger = null)
        => new(items, columns, logger ?? s_nullLogger,
            new SimpleTableSortComparerResolver<TableTestItem>(
                Substitute.For<ILogger<SimpleTableSortComparerResolver<TableTestItem>>>()));

    [Fact]
    public async Task Returns_items_in_original_order_when_no_sorting_applied()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(3, "Banana"),
            new(1, "Apple"),
            new(2, "Cherry")
        };

        var provider = CreateProvider(items, () => []);

        // Act
        var result = await provider.GetItemsAsync(s_noSortingNoRange, CancellationToken.None);

        // Assert
        result.Items.Should().ContainInOrder(items.ToList());
    }

    [Fact]
    public async Task Sorts_ascending_by_single_column()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(3, "Banana"),
            new(1, "Apple"),
            new(2, "Cherry")
        };

        var column = new SortableColumn("Name", x => x.TestValue);

        var context = new ItemsProviderContext { SortingState = SortingState.Empty.WithColumnSorting("Name", ascending: true) };

        // Act
        var result = await CreateProvider(items, () => [column])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        result.Items.Select(x => x.TestValue).Should().ContainInOrder("Apple", "Banana", "Cherry");
    }

    [Fact]
    public async Task Sorts_descending_by_single_column()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(3, "Banana"),
            new(1, "Apple"),
            new(2, "Cherry")
        };

        var column = new SortableColumn("Name", x => x.TestValue);

        var context = new ItemsProviderContext { SortingState = SortingState.Empty.WithColumnSorting("Name", ascending: false) };

        // Act
        var result = await CreateProvider(items, () => [column])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        result.Items.Select(x => x.TestValue).Should().ContainInOrder("Cherry", "Banana", "Apple");
    }

    [Fact]
    public async Task Sorting_descending_reverses_rows_tied_on_the_sort_key()
    {
        // Arrange: every row shares the sorted value, so only the tie rule shows
        var items = new List<TableTestItem>
        {
            new(1, "Shared"),
            new(2, "Shared"),
            new(3, "Shared")
        };

        var column = new SortableColumn("Name", x => x.TestValue);

        var ascending = new ItemsProviderContext { SortingState = SortingState.Empty.WithColumnSorting("Name", ascending: true) };
        var descending = new ItemsProviderContext { SortingState = SortingState.Empty.WithColumnSorting("Name", ascending: false) };

        var provider = CreateProvider(items, () => [column]);

        // Act
        var ascendingResult = await provider.GetItemsAsync(ascending, CancellationToken.None);
        var descendingResult = await provider.GetItemsAsync(descending, CancellationToken.None);

        // Assert
        ascendingResult.Items.Select(x => x.TestKey).Should().ContainInOrder(1, 2, 3);
        descendingResult.Items.Select(x => x.TestKey).Should().ContainInOrder(3, 2, 1);
    }

    [Fact]
    public async Task Sorting_descending_reverses_tied_rows_inside_swapped_key_groups()
    {
        // Arrange: two key groups, each holding a tie, so the assertion separates "groups swapped" from "ties reversed"
        var items = new List<TableTestItem>
        {
            new(1, "Apple"),
            new(2, "Apple"),
            new(3, "Banana"),
            new(4, "Banana")
        };

        var column = new SortableColumn("Name", x => x.TestValue);

        var context = new ItemsProviderContext { SortingState = SortingState.Empty.WithColumnSorting("Name", ascending: false) };

        // Act
        var result = await CreateProvider(items, () => [column])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        result.Items.Select(x => x.TestKey).Should().ContainInOrder(4, 3, 2, 1);
    }

    [Fact]
    public async Task Multi_column_sort_reverses_rows_tied_on_every_key_when_last_sorting_is_descending()
    {
        // Arrange: rows 1 and 2 tie on both sorted columns; row 3 differs on the primary key
        var items = new List<TableTestItem>
        {
            new(1, "Shared"),
            new(2, "Shared"),
            new(3, "Unique")
        };

        var nameColumn = new SortableColumn("Name", x => x.TestValue);
        var flagColumn = new SortableColumn("Flag", _ => 0);

        var context = new ItemsProviderContext
        {
            SortingState = SortingState.Empty.WithColumnSorting("Name", ascending: true).WithColumnSorting("Flag", ascending: false)
        };

        // Act
        var result = await CreateProvider(items, () => [nameColumn, flagColumn])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        result.Items.Select(x => x.TestKey).Should().ContainInOrder(2, 1, 3);
    }

    [Fact]
    public async Task Multi_column_sort_keeps_rows_tied_on_every_key_in_source_order_when_last_sorting_is_ascending()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(1, "Shared"),
            new(2, "Shared"),
            new(3, "Unique")
        };

        var nameColumn = new SortableColumn("Name", x => x.TestValue);
        var flagColumn = new SortableColumn("Flag", _ => 0);

        var context = new ItemsProviderContext
        {
            SortingState = SortingState.Empty.WithColumnSorting("Name", ascending: true).WithColumnSorting("Flag", ascending: true)
        };

        // Act
        var result = await CreateProvider(items, () => [nameColumn, flagColumn])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        result.Items.Select(x => x.TestKey).Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public async Task Filtered_set_keeps_source_order_when_sorting_descending()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(1, "Shared"),
            new(2, "Shared"),
            new(3, "Shared")
        };

        var column = new SortableColumn("Name", x => x.TestValue);

        var context = new ItemsProviderContext { SortingState = SortingState.Empty.WithColumnSorting("Name", ascending: false) };

        var provider = CreateProvider(items, () => [column]);

        // Act
        await provider.GetItemsAsync(context, CancellationToken.None);

        // Assert
        provider.FilteredItems.Select(x => x.TestKey).Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public async Task Multi_column_sort_applies_primary_then_secondary()
    {
        // Arrange
        // Three items: two share the same primary sort value
        var items = new List<TableTestItem>
        {
            new(1, "Shared"),
            new(3, "Shared"),
            new(2, "Unique")
        };

        var nameColumn = new SortableColumn("Name", x => x.TestValue);
        var keyColumn = new SortableColumn("Key", x => x.TestKey);

        // Primary: Name ascending — "Shared" items come before "Unique"
        // Secondary: Key ascending — within "Shared" group, key 1 before key 3
        var context = new ItemsProviderContext
        {
            SortingState = SortingState.Empty.WithColumnSorting("Name", ascending: true).WithColumnSorting("Key", ascending: true)
        };

        // Act
        var result = await CreateProvider(items, () => [nameColumn, keyColumn])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        result.Items.Select(x => x.TestKey).Should().ContainInOrder(1, 3, 2);
    }

    [Fact]
    public async Task Comparer_on_secondary_column_is_honored_while_primary_uses_default()
    {
        // Arrange — two items share the primary sort value, so the secondary column decides their order
        var items = new List<TableTestItem>
        {
            new(1, "Shared"),
            new(3, "Shared"),
            new(2, "Unique")
        }.ToList();

        var nameColumn = new SortableColumn("Name", x => x.TestValue);
        var keyColumn = new SortableColumn("Key", x => x.TestKey, new ReverseInt32Comparer());

        var context = new ItemsProviderContext
        {
            SortingState = SortingState.Empty.WithColumnSorting("Name", ascending: true).WithColumnSorting("Key", ascending: true)
        };

        // Act
        var result = await CreateProvider(items, () => [nameColumn, keyColumn])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert — within the "Shared" group the reverse comparer puts key 3 before key 1
        result.Items.Select(x => x.TestKey).Should().ContainInOrder(3, 1, 2);
    }

    [Fact]
    public async Task Comparer_on_primary_column_is_honored_while_secondary_uses_default()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(3, "Shared"),
            new(1, "Shared"),
            new(2, "Unique")
        }.ToList();

        var nameColumn = new SortableColumn("Name", x => x.TestValue, new ReverseStringComparer());
        var keyColumn = new SortableColumn("Key", x => x.TestKey);

        var context = new ItemsProviderContext
        {
            SortingState = SortingState.Empty.WithColumnSorting("Name", ascending: true).WithColumnSorting("Key", ascending: true)
        };

        // Act
        var result = await CreateProvider(items, () => [nameColumn, keyColumn])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert — reversed primary puts "Unique" first; the default secondary orders the "Shared" group 1, 3
        result.Items.Select(x => x.TestKey).Should().ContainInOrder(2, 1, 3);
    }

    [Fact]
    public async Task Comparer_without_sort_expression_leaves_column_non_sortable()
    {
        // Arrange — a comparer alone does not make a column sortable; the misconfigured sorting entry is
        // skipped like any other non-sortable column, never applied and never throwing
        var items = new List<TableTestItem> { new(2, "Banana"), new(1, "Apple") }.ToList();

        var column = new ComparerOnlyColumn("Name", new ReverseStringComparer());
        var context = new ItemsProviderContext { SortingState = SortingState.Empty.WithColumnSorting("Name", ascending: true) };

        // Act
        var act = async () => await CreateProvider(items, () => [column])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        var result = await act.Should().NotThrowAsync();

        result.Subject.Items.Should().ContainInOrder(items.ToList());
    }

    [Fact]
    public async Task Unknown_column_id_in_sorting_is_silently_skipped()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(3, "Banana"),
            new(1, "Apple"),
        };

        var context = new ItemsProviderContext { SortingState = SortingState.Empty.WithColumnSorting("DoesNotExist", ascending: true) };

        // Act
        var result = await CreateProvider(items, () => [])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        result.Items.Should().ContainInOrder(items.ToList());
    }

    [Fact]
    public async Task Non_sortable_column_in_sorting_state_is_skipped_without_throwing()
    {
        // Arrange
        List<TableTestItem> items = [new(2, "Banana"), new(1, "Apple")];

        var column = new NonSortableColumn("Name");
        var context = new ItemsProviderContext { SortingState = SortingState.Empty.WithColumnSorting("Name", ascending: true) };

        // Act
        var act = async () => await CreateProvider(items, () => [column])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        // misconfiguration must not tear down the circuit: the bad column is skipped and items keep their original order
        var result = await act.Should().NotThrowAsync();

        result.Subject.Items.Should().ContainInOrder(items.ToList());
    }

    [Fact]
    public async Task Null_item_range_returns_all_items()
    {
        // Arrange
        var items = Enumerable.Range(1, 10)
            .Select(i => new TableTestItem(i, i.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        // Act
        var result = await CreateProvider(items, () => [])
            .GetItemsAsync(s_noSortingNoRange, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(10);
    }

    [Fact]
    public async Task Item_range_applies_skip_and_take()
    {
        // Arrange
        var items = Enumerable.Range(1, 10)
            .Select(i => new TableTestItem(i, i.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var context = new ItemsProviderContext { ItemRange = new ItemRange(Skip: 3, Take: 4) };

        // Act
        var result = await CreateProvider(items, () => [])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        result.Items.Select(x => x.TestKey).Should().ContainInOrder(4, 5, 6, 7);
    }

    [Fact]
    public async Task Total_item_count_reflects_full_set_regardless_of_range()
    {
        // Arrange
        var items = Enumerable.Range(1, 10)
            .Select(i => new TableTestItem(i, i.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var context = new ItemsProviderContext { ItemRange = new ItemRange(Skip: 5, Take: 2) };

        // Act
        var result = await CreateProvider(items, () => [])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        result.TotalItemCount.Should().Be(10);
    }

    [Fact]
    public async Task Filters_in_memory_and_total_count_reflects_the_filtered_set()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Apple"), new(2, "Banana"), new(3, "Avocado") };

        var column = new NonSortableColumn("Value");
        var filter = FilterState.Empty.WithColumnFilter(
            new SimpleTableContainsColumnFilter<TableTestItem>("Value", "ban", x => x.TestValue));

        var context = new ItemsProviderContext { FilterState = filter };

        // Act
        var result = await CreateProvider(items, () => [column])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        result.Items.Select(x => x.TestValue).Should().ContainSingle().Which.Should().Be("Banana");
        result.TotalItemCount.Should().Be(1);
    }

    [Fact]
    public async Task Multiple_column_filters_combine_with_and()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Banana"), new(2, "Banana"), new(3, "Apple") };

        var valueColumn = new NonSortableColumn("Value");
        var keyColumn = new NonSortableColumn("Key");

        var filter = FilterState.Empty
            .WithColumnFilter(new SimpleTableContainsColumnFilter<TableTestItem>("Value", "ban", x => x.TestValue))
            .WithColumnFilter(new SimpleTableContainsColumnFilter<TableTestItem>(
                "Key", "2", x => x.TestKey.ToString(CultureInfo.InvariantCulture)));

        var context = new ItemsProviderContext { FilterState = filter };

        // Act
        var result = await CreateProvider(items, () => [valueColumn, keyColumn])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        result.Items.Select(x => x.TestKey).Should().ContainSingle().Which.Should().Be(2);
        result.TotalItemCount.Should().Be(1);
    }

    [Fact]
    public async Task Wrong_type_filter_on_a_registered_column_is_logged_and_skipped()
    {
        // Arrange — TestColumnFilter is an IColumnFilter but not an ISimpleTableColumnFilter<TItem>. The
        // provider must log an error and skip it, never throw — a throw would kill the circuit.
        var logger = Substitute.For<ILogger<SimpleTableItemsProvider<TableTestItem>>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var items = new List<TableTestItem> { new(1, "Apple"), new(2, "Banana") };
        var column = new NonSortableColumn("Value");
        var filter = FilterState.Empty.WithColumnFilter(new TestColumnFilter("Value"));

        var context = new ItemsProviderContext { FilterState = filter };

        // Act
        var act = async () => await CreateProvider(items, () => [column], logger)
            .GetItemsAsync(context, CancellationToken.None);

        // Assert — no throw, every row survives (filter skipped), and an error was logged
        var result = await act.Should().NotThrowAsync();
        result.Subject.Items.Should().HaveCount(2);
        logger.ReceivedCalls()
            .Should().Contain(call => call.GetMethodInfo().Name == nameof(ILogger.Log)
                && (LogLevel)call.GetArguments()[0]! == LogLevel.Error);
    }

    [Fact]
    public async Task Wrong_type_filter_log_is_deduplicated_across_provides()
    {
        // Arrange
        var logger = Substitute.For<ILogger<SimpleTableItemsProvider<TableTestItem>>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        var items = new List<TableTestItem> { new(1, "Apple") };
        var column = new NonSortableColumn("Value");
        var filter = FilterState.Empty.WithColumnFilter(new TestColumnFilter("Value"));
        var context = new ItemsProviderContext { FilterState = filter };

        var provider = CreateProvider(items, () => [column], logger);

        // Act — the same misconfiguration provided twice
        await provider.GetItemsAsync(context, CancellationToken.None);
        await provider.GetItemsAsync(context, CancellationToken.None);

        // Assert — logged only once for the (column, type) pair
        logger.ReceivedCalls()
            .Count(call => call.GetMethodInfo().Name == nameof(ILogger.Log)
                && (LogLevel)call.GetArguments()[0]! == LogLevel.Error)
            .Should().Be(1);
    }

    [Fact]
    public async Task Global_filter_matches_the_whole_item_and_applies_without_a_registered_column()
    {
        // Arrange — a global filter carries no column id, so it applies even with no columns registered
        var items = new List<TableTestItem> { new(1, "Apple"), new(2, "Banana"), new(3, "Avocado") };
        var filter = FilterState.Empty.WithGlobalFilter(new TestGlobalFilter("ban"));

        var context = new ItemsProviderContext { FilterState = filter };

        // Act
        var result = await CreateProvider(items, () => [])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        result.Items.Select(x => x.TestValue).Should().ContainSingle().Which.Should().Be("Banana");
        result.TotalItemCount.Should().Be(1);
    }

    [Fact]
    public async Task Global_filter_combines_with_a_column_filter_using_and()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Banana"), new(2, "Banana"), new(3, "Apple") };

        var valueColumn = new NonSortableColumn("Value");

        // Column filter keeps the "Banana" rows (keys 1, 2); global filter keeps keys >= 2 (keys 2, 3).
        // Their AND leaves only key 2.
        var filter = FilterState.Empty
            .WithColumnFilter(new SimpleTableContainsColumnFilter<TableTestItem>("Value", "ban", x => x.TestValue))
            .WithGlobalFilter(new TestSecondGlobalFilter(minKey: 2));

        var context = new ItemsProviderContext { FilterState = filter };

        // Act
        var result = await CreateProvider(items, () => [valueColumn])
            .GetItemsAsync(context, CancellationToken.None);

        // Assert
        result.Items.Select(x => x.TestKey).Should().ContainSingle().Which.Should().Be(2);
        result.TotalItemCount.Should().Be(1);
    }

    [Fact]
    public void Filtered_set_is_empty_before_the_first_provide()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Apple"), new(2, "Banana") };

        // Act
        var provider = CreateProvider(items, () => []);

        // Assert — a fresh provider exposes an empty Filtered Set until the first provide lands
        provider.FilteredItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Provide_without_a_filter_caches_the_whole_dataset()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Apple"), new(2, "Banana"), new(3, "Cherry") };

        var provider = CreateProvider(items, () => []);

        // Act
        await provider.GetItemsAsync(s_noSortingNoRange, CancellationToken.None);

        // Assert — with no filter active, the Filtered Set is the whole Dataset
        provider.FilteredItems.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task Provide_with_a_range_caches_the_whole_filtered_set()
    {
        // Arrange — a Virtualize-shaped provide: a ranged window over a filtered set
        var items = new List<TableTestItem>
        {
            new(1, "Apple"),
            new(2, "Banana"),
            new(3, "Avocado"),
            new(4, "Apricot")
        };

        var column = new NonSortableColumn("Value");
        var filter = FilterState.Empty.WithColumnFilter(
            new SimpleTableContainsColumnFilter<TableTestItem>("Value", "ap", x => x.TestValue));

        var context = new ItemsProviderContext { FilterState = filter, ItemRange = new ItemRange(Skip: 0, Take: 1) };

        var provider = CreateProvider(items, () => [column]);

        // Act
        var result = await provider.GetItemsAsync(context, CancellationToken.None);

        // Assert — the returned items are the window, but the cached Filtered Set is every filtered row
        result.Items.Should().ContainSingle();
        provider.FilteredItems.Select(x => x.TestKey).Should().BeEquivalentTo([1, 4]);
    }

    // Holds the expression rather than a delegate producing the key, so the key type the provider reads off it
    // is the concrete one a column written in markup declares.
    private sealed class SortableColumn(string id, Expression<Func<TableTestItem, object>> sortExpression,
        IComparer? sortComparer = null)
            : IAdvancedTableColumn<TableTestItem>, IHasSortExpression<TableTestItem>
    {
        public string Id => id;

        public RenderFragment Header => _ => { };

        public RenderFragment<TableTestItem> CellContent => _ => _ => { };

        public int? DefaultWidth { get; }

        public bool Visible { get; set; } = true;

        public PinSide PinSide => PinSide.None;

        public bool ShowInColumnChooser => throw new NotImplementedException();

        public bool Sortable => true;

        public Expression<Func<TableTestItem, object>>? SortExpression => sortExpression;

        public IComparer? SortComparer => sortComparer;

        public ColumnState? State => throw new NotImplementedException();

        public void SetState(ColumnState? columnState)
            => throw new NotImplementedException();
    }

    private sealed class ComparerOnlyColumn(string id, IComparer sortComparer)
        : IAdvancedTableColumn<TableTestItem>, IHasSortExpression<TableTestItem>
    {
        public string Id => id;

        public RenderFragment Header => _ => { };

        public RenderFragment<TableTestItem> CellContent => _ => _ => { };

        public int? DefaultWidth { get; }

        public bool Visible { get; set; } = true;

        public PinSide PinSide => PinSide.None;

        public bool ShowInColumnChooser => throw new NotImplementedException();

        public bool Sortable => false;

        public Expression<Func<TableTestItem, object>>? SortExpression => null;

        public IComparer? SortComparer => sortComparer;

        public ColumnState? State => throw new NotImplementedException();

        public void SetState(ColumnState? columnState)
            => throw new NotImplementedException();
    }

    private sealed class ReverseStringComparer : Comparer<string>
    {
        public override int Compare(string? x, string? y)
            => StringComparer.Ordinal.Compare(y, x);
    }

    private sealed class ReverseInt32Comparer : Comparer<int>
    {
        public override int Compare(int x, int y)
            => y.CompareTo(x);
    }

    private sealed class NonSortableColumn(string id) : IAdvancedTableColumn<TableTestItem>
    {
        public string Id => id;

        public RenderFragment Header => _ => { };

        public RenderFragment<TableTestItem> CellContent => _ => _ => { };

        public int? DefaultWidth { get; }

        public bool Visible { get; set; } = true;

        public PinSide PinSide => PinSide.None;

        public bool ShowInColumnChooser => throw new NotImplementedException();

        public ColumnState? State => throw new NotImplementedException();

        public void SetState(ColumnState? columnState)
            => throw new NotImplementedException();
    }
}
