using System.Collections;
using ViciOne.Ui.Blazor.Components.Popup.Components;

namespace ViciOne.Ui.Blazor.Components.Popup.Services;

internal sealed class PopupRegistry : IPopupRegistry
{
    private readonly HashSet<IPopup> _items = [];

    public event Action? Changed;

    public void Add(IPopup popup)
    {
        if (_items.Add(popup))
            Changed?.Invoke();
    }

    public void Remove(IPopup popup)
    {
        if (_items.Remove(popup))
            Changed?.Invoke();
    }

    public IEnumerator<IPopup> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
