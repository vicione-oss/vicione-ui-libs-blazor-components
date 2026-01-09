namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

/// <summary>
/// Equality comparer for a specific type of property values
/// </summary>
public interface IPropertyValueEqualityComparer;

/// <summary>
/// Equality comparer for property values of type <typeparamref name="TPropertyValue"/>
/// </summary>
public interface IPropertyValueEqualityComparer<in TPropertyValue>
    : IPropertyValueEqualityComparer, IEqualityComparer<TPropertyValue>
        where TPropertyValue : allows ref struct;
