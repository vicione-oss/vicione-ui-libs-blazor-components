using Bogus;

namespace Shared.Pages.Dialog.Components;

public sealed partial class DialogPage
{
    private int? _dialogMinimumWidth;
    private int? _dialogWidth = 540;
    private int? _dialogHeight = 480;
    private bool _dialogWithHeaderToolbar = true;
    private bool _dialogPreventBrowserContextMenu = true;
    private bool _dialogVisible;
    private bool _dialogWithBodyTextLayout = true;

    private readonly Faker _faker = new();

    private void CloseDialog()
        => _dialogVisible = false;
}
