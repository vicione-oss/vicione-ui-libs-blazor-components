namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

/// <summary>
/// Describes a property whose value can be set from a range of selectable values.
/// </summary>
public interface ISelectionPropertyDescriptor;

/// <inheritdoc cref="ISelectionPropertyDescriptor"/>
public interface ISelectionPropertyDescriptor<TInstance, TPropertyValue>
    : IPropertyDescriptor<TInstance, TPropertyValue>, ISelectionPropertyDescriptor
{
    /// <summary>
    /// Function that returns a range of values selectable from the associated editor and
    /// allowed to be used in write operations like <see cref="IPropertyDescriptor{TInstance, TPropertyValue}.SetValue"/>.
    /// </summary>
    /// <remarks>
    /// When no selectable values are returned, the associated editor will be disabled and
    /// a suitable information tooltip will be displayed next to it to inform the user on why
    /// the editor is disabled.
    /// </remarks>
    Func<TInstance, IEnumerable<ISelectableValue<TPropertyValue>>> GetSelectableValues { get; }
}
