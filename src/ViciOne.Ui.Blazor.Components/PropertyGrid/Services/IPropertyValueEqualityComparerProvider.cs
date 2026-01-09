namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

internal interface IPropertyValueEqualityComparerProvider<TContext>
{
    object GetPropertyValueEqualityComparer(Type valueType);
    IEqualityComparer<TPropertyValue> GetPropertyValueEqualityComparer<TPropertyValue>();
}
