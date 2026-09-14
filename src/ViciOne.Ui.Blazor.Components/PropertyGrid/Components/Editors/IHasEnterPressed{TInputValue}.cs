using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components.Editors;

internal interface IHasEnterPressed<TInputValue>
{
    /// <summary>
    /// Raised when Enter was pressed
    /// </summary>
    EventCallback<TInputValue> EnterPressed { get; set; }
}
