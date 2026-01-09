namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

/// <summary>
/// Models an API to read from / write to a set of similar properties that allow selection from a predefined set of values.
/// </summary>
internal interface ISelectionPropertyGridItem<TPropertyValue> : IPropertyGridItem
{
    /// <returns>
    /// List of unified selectable values for the underlying properties
    /// or <see langword="null"/> when no unified selectable values could be found.
    /// </returns>
    List<ISelectableValue<TPropertyValue>>? GetUnifiedSelectableValues();
}
