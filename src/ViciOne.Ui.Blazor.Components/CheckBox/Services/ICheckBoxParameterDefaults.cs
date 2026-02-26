namespace ViciOne.Ui.Blazor.Components.CheckBox.Services;

/// <summary>
/// Implements the application of parameter defaults to a checkbox component.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public interface ICheckBoxParameterDefaults<TValue>
{
    /// <summary>
    /// Applies default values to parameters of the specified <paramref name="checkbox"/>.
    /// </summary>
    void Apply(ICheckBox<TValue> checkbox);
}
