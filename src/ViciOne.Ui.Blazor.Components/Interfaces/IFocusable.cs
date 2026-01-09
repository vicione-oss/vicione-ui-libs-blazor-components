namespace ViciOne.Ui.Blazor.Components.Interfaces;

/// <summary>
/// Component that can receive focus
/// </summary>
public interface IFocusable
{
    /// <summary>
    /// Moves focus to the component
    /// </summary>
    Task FocusAsync();
}
