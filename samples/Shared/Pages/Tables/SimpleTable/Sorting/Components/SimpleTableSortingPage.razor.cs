using System.Globalization;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;

namespace Shared.Pages.Tables.SimpleTable.Sorting.Components;

public sealed partial class SimpleTableSortingPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    // Natural (numeric-aware) string comparer straight from .NET — no custom comparer code needed.
    private static readonly StringComparer s_naturalComparer =
        CultureInfo.InvariantCulture.CompareInfo.GetStringComparer(CompareOptions.NumericOrdering);

    // Codes whose default lexicographic order ("FB1", "FB10", "FB100", "FB2", …) visibly differs from the
    // natural order the comparer produces ("FB1", "FB2", "FB3", "FB10", …).
    private static readonly List<ExampleTableItem> s_codeItems =
    [
        .. new[] { "FB10", "FB2", "FB100", "FB1", "FB20", "FB3", "FB11" }.Select(code => new ExampleTableItem
        {
            Id = Guid.NewGuid(),
            Key = code,
            Value = $"Function block {code[2..]}",
            Timestamp = DateTimeOffset.Now,
            Quantity = int.Parse(code[2..], CultureInfo.InvariantCulture)
        })
    ];

    // Three keys with three rows each, so every row ties with two others.
    private static readonly List<ExampleTableItem> s_tiedItems =
    [
        .. new[] { "Alpha", "Beta", "Gamma" }
            .SelectMany(key => Enumerable.Repeat(key, 3))
            .Select((key, index) => new ExampleTableItem
            {
                Id = Guid.NewGuid(),
                Key = key,
                Value = $"source position {index + 1}",
                Timestamp = DateTime.Now.AddMinutes(index),
                Quantity = index + 1
            })
    ];

    // One instance, handed over on every render and never replaced, so the table applies it once and owns the
    // sorting from then on.
    private readonly SortingState _seedSortingState = SortingState.Empty.WithColumnSorting("Value", ascending: true);

    // Observe only.
    private SortingState _observedSortingState = SortingState.Empty;

    // Both ends of a binding: this page writes it to drive the table, the table writes it back on every change.
    private SortingState _boundSortingState = SortingState.Empty;

    private SimpleTable<ExampleTableItem>? _pushTargetTable;

    // Mirrored from SortingStateChanged purely so the page can list what the table applies.
    private SortingState _pushedSortingState = SortingState.Empty;

    private void ObservedSortingStateChanged(SortingState sortingState)
        => _observedSortingState = sortingState;

    private void PushedSortingStateChanged(SortingState sortingState)
        => _pushedSortingState = sortingState;

    // Writing the bound field is the whole command: the new instance reaches the table as a parameter.
    private void SortBoundTableByKey()
        => _boundSortingState = SortingState.Empty.WithColumnSorting("Key", ascending: true);

    private void ClearBoundSorting()
        => _boundSortingState = SortingState.Empty;

    // A call is applied whatever the parameter channel has already delivered, so resetting works even when the
    // table has seen the empty sorting before.
    private Task ResetSortingAsync()
        => _pushTargetTable?.SetSortingStateAsync(SortingState.Empty) ?? Task.CompletedTask;
}
