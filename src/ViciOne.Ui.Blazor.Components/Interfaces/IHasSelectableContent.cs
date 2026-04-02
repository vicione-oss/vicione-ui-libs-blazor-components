namespace ViciOne.Ui.Blazor.Components.Interfaces;

/// <summary>
/// Component that has selectable content
/// </summary>
internal interface IHasSelectableContent
{
    /// <summary>
    /// Select the content of the component
    /// </summary>
    Task SelectContentAsync();
}
