using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components.Editors;

internal interface IHasValueParseError
{
    /// <summary>
    /// Event callback invoked when input could not be parsed into a property value.
    /// </summary>
    /// <remarks>
    /// The associated error message is passed to the event callback.
    /// </remarks>
    EventCallback<string> ValueParseError { get; set; }
}
