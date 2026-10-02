using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.FocusTrap.Components;

/// <summary>
/// A component that can keep the focus inside itself.
/// </summary>
/// <remarks>
/// Elements the focus may still move into while they are stacked above the trap are marked with the
/// <c>data-focus-trap-exempt</c> attribute.
/// </remarks>
internal interface IFocusTrappable
{
    /// <summary>
    /// Gets the element reference of the component the focus is kept inside of.
    /// </summary>
    ElementReference GetElementReference();
}
