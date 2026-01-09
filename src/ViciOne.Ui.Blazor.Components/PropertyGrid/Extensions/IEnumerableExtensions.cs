namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;

internal static class IEnumerableExtensions
{
    public static IEnumerable<object> OfType(this IEnumerable<object> objects, Type objectType)
    {
        var ofTypeDelegate = Enumerable.OfType<object>;

        var ofTypeMethod = ofTypeDelegate.Method
            .GetGenericMethodDefinition()
            .MakeGenericMethod(objectType);

        var result = ofTypeMethod.Invoke(null, [objects]);

        if (result is IEnumerable<object> enumerable)
            return enumerable;
        else
            throw new InvalidOperationException();
    }

    public static IEnumerable<object> Cast(this IEnumerable<object> objects, Type objectType)
    {
        var castDelegate = Enumerable.Cast<object>;

        var castMethod = castDelegate.Method
            .GetGenericMethodDefinition()
            .MakeGenericMethod(objectType);

        var result = castMethod.Invoke(null, [objects]);

        if (result is IEnumerable<object> enumerable)
            return enumerable;
        else
            throw new InvalidOperationException();
    }

    public static IReadOnlyList<object> ToReadOnlyList(this IEnumerable<object> objects, Type objectType)
    {
        var toListDelegate = Enumerable.ToList<object>;

        var toListMethod = toListDelegate.Method
            .GetGenericMethodDefinition()
            .MakeGenericMethod(objectType);

        var result = toListMethod.Invoke(null, [objects]);

        if (result is IReadOnlyList<object> readOnlyList)
            return readOnlyList;
        else
            throw new InvalidOperationException();
    }
}
