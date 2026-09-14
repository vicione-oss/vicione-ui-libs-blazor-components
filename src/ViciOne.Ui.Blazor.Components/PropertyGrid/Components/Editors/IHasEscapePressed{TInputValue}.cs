using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components.Editors;

internal interface IHasEscapePressed<TInputValue>
{
    /// <summary>
    /// Raised when Escape was pressed
    /// </summary>
    EventCallback<TInputValue> EscapePressed { get; set; }
}
