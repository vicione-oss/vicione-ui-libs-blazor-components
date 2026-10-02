using ViciOne.Ui.Blazor.Components.FocusTrap.Components;

namespace ViciOne.Ui.Blazor.Components.FocusTrap.Services;

internal interface IFocusTrap
{
    /// <summary>
    /// Keeps the focus inside the specified <paramref name="focusTrappable"/>, following it to a new element
    /// when it has been rendered anew.
    /// </summary>
    /// <remarks>
    /// Does nothing while the element of <paramref name="focusTrappable"/> is not rendered yet.
    /// </remarks>
    Task AttachAsync(IFocusTrappable focusTrappable);

    /// <summary>
    /// Releases the focus trap of the specified <paramref name="focusTrappable"/>, if any.
    /// </summary>
    Task RemoveAsync(IFocusTrappable focusTrappable);
}
