namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

/// <summary>
/// Describes a property of <typeparamref name="TInstance"/> whereas the property value is <typeparamref name="TPropertyValue"/>
/// </summary>
public class SelectionPropertyDescriptor<TInstance, TPropertyValue>
    : PropertyDescriptor<TInstance, TPropertyValue>, ISelectionPropertyDescriptor<TInstance, TPropertyValue>
{
    /// <inheritdoc/>
    public required Func<TInstance, IEnumerable<ISelectableValue<TPropertyValue>>> GetSelectableValues { get; set; }
}
