namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

internal sealed class PropertyValueEqualityComparerProvider<TContext>(IEnumerable<IPropertyValueEqualityComparer> valueEqualityComparers)
    : IPropertyValueEqualityComparerProvider<TContext>
{
    private readonly Dictionary<Type, object> _valueEqualityComparerCache = [];

    public object GetPropertyValueEqualityComparer(Type valueType)
    {
        if (_valueEqualityComparerCache.TryGetValue(valueType, out var valueEqualityComparer))
            return valueEqualityComparer;

        var methodDelegate = GetPropertyValueEqualityComparer<object>;
        var methodInfo = methodDelegate.Method.GetGenericMethodDefinition().MakeGenericMethod(valueType);

        var result = methodInfo.Invoke(this, [])
            ?? throw new InvalidOperationException($"Could not get an equality comparer for type {valueType.Name}");
        _valueEqualityComparerCache.Add(valueType, result);

        return result;
    }

    public IEqualityComparer<TPropertyValue> GetPropertyValueEqualityComparer<TPropertyValue>()
    {
        var result = valueEqualityComparers.OfType<IEqualityComparer<TPropertyValue>>()
            .FirstOrDefault();

        result ??= EqualityComparer<TPropertyValue>.Default;

        return result;
    }
}


