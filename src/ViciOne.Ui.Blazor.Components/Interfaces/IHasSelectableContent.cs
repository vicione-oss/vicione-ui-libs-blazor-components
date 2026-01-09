namespace ViciOne.Ui.Blazor.Components.Interfaces;

/// <summary>
/// Component that can has selectable content
/// </summary>
internal interface IHasSelectableContent
{
    /// <summary>
    /// Select the content of the component
    /// </summary>
    Task SelectContentAsync();
}
