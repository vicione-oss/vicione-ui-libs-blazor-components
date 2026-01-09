using ViciOne.Ui.Blazor.Components.Popup.Components;

namespace ViciOne.Ui.Blazor.Components.Popup.Services;

internal interface IPopupRegistry : IEnumerable<IPopup>
{
    event Action? Changed;

    void Add(IPopup popup);
    void Remove(IPopup popup);
}
