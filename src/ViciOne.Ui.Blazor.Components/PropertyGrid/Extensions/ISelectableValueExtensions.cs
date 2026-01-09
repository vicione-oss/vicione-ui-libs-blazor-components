using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;

internal static class ISelectableValueExtensions
{
    public static IEnumerable<ISelectableValue<TPropertyValue>> Except<TPropertyValue>(this IEnumerable<ISelectableValue<TPropertyValue>> first,
        IEnumerable<ISelectableValue<TPropertyValue>> second, IEqualityComparer<TPropertyValue> valueEqualityComparer)
    {
        var differentValues = new List<ISelectableValue<TPropertyValue>>();
        var otherPossibleValues = second.ToList();

        foreach (var possibleValue in first)
        {
            if (!otherPossibleValues.Any(otherPossibleValue => valueEqualityComparer.Equals(otherPossibleValue.Value, possibleValue.Value)))
                yield return possibleValue;
        }
    }
}
