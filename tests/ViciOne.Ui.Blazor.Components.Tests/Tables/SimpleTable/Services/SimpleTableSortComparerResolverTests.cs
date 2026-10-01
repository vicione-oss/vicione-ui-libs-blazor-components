using System.Collections;
using Microsoft.Extensions.Logging;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Services;

public sealed class SimpleTableSortComparerResolverTests
{
    private static SimpleTableSortComparerResolver<TableTestItem> CreateResolver()
        => new(Substitute.For<ILogger<SimpleTableSortComparerResolver<TableTestItem>>>());

    [Fact]
    public void Comparer_stated_for_the_exact_key_type_is_resolved_and_used_for_comparison()
    {
        // Arrange — ReverseStringComparer : Comparer<string> matches TestValue's string keys exactly
        var comparer = new ReverseStringComparer();
        var resolver = CreateResolver();

        // Act
        var resolved = resolver.Resolve("Name", comparer, x => x.TestValue);

        // Assert
        resolved.Should().NotBeNull();
        resolved!.Compare("Apple", "Banana").Should().Be(comparer.Compare("Apple", "Banana"));
    }

    [Fact]
    public void Comparer_for_an_unrelated_key_type_is_skipped()
    {
        // Arrange — a comparer for numeric keys on a column sorting by text can only be a misconfiguration,
        // and applying it would throw out of the sort
        var resolver = CreateResolver();

        // Act
        var resolved = resolver.Resolve("Name", new ReverseInt32Comparer(), x => x.TestValue);

        // Assert — the column falls back to the default comparison instead of failing the provide
        resolved.Should().BeNull();
    }

    [Fact]
    public void Comparer_for_a_wider_key_type_is_honored()
    {
        // Arrange — a comparer written against object keys states a key type every column's keys fit into
        var comparer = Comparer<object>.Create((x, y) => StringComparer.Ordinal.Compare(y?.ToString(), x?.ToString()));
        var resolver = CreateResolver();

        // Act
        var resolved = resolver.Resolve("Name", comparer, x => x.TestValue);

        // Assert
        resolved.Should().NotBeNull();
        resolved!.Compare("Apple", "Banana").Should().Be(comparer.Compare("Apple", "Banana"));
    }

    [Fact]
    public void Comparer_stating_no_key_type_is_honored()
    {
        // Arrange — a non-generic comparer states no key type, so there is nothing to check it against and it
        // is applied as it is
        var comparer = new ReverseNonGenericComparer();
        var resolver = CreateResolver();

        // Act
        var resolved = resolver.Resolve("Key", comparer, x => x.TestKey);

        // Assert
        resolved.Should().NotBeNull();
        resolved!.Compare(1, 2).Should().Be(comparer.Compare(1, 2));
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

    // The shape the key type check cannot inspect: non-generic, so it states no key type and takes its
    // arguments as object.
    private sealed class ReverseNonGenericComparer : IComparer
    {
        public int Compare(object? x, object? y)
            => Comparer<object>.Default.Compare(y, x);
    }
}
