namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models;

/// <summary>
/// Value selectable from a list of values
/// </summary>
public interface ISelectableValue<TPropertyValue>
{
    /// <summary>
    /// Property value associated with this selectable value
    /// </summary>
    TPropertyValue Value { get; }

    /// <summary>
    /// Text that should be displayed in the list of values for this selectable value
    /// </summary>
    string Text { get; }
}
