using Bogus;

namespace Shared.Pages.Popup.Components;

public sealed partial class PopupPage
{
    private bool _popupVisible;
    private bool _popupCloseOnEscape = true;
    private IEnumerable<string> _popupTags = [];
    private int? _popupMinimumWidth;
    private int? _popupWidth = 640;
    private int? _popupHeight = 480;
    private bool _popupWithInvertedModifier;
    private bool _popupShowBackdrop;
    private bool _popupMoveable;
    private bool _popupPreventBrowserContextMenu;

    private readonly Faker _faker = new();
}
