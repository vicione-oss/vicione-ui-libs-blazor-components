namespace ViciOne.Ui.Blazor.Components.CheckBox;

/// <summary>
/// Component that allows users to toggle between two states
/// </summary>
public interface ICheckBox<TValue>
{
    /// <summary>
    /// Specifies the value that corresponds to the checked state
    /// </summary>
    TValue ValueChecked { get; set; }

    /// <summary>
    /// Specifies the value that corresponds to the unchecked state
    /// </summary>
    TValue ValueUnchecked { get; set; }

    /// <summary>
    /// Specifies the value that corresponds to the indeterminate state
    /// </summary>
    TValue ValueIndeterminate { get; set; }

    /// <summary>
    /// Specifies whether the check-box supports the indeterminate state
    /// </summary>
    bool AllowIndeterminateState { get; set; }
}
