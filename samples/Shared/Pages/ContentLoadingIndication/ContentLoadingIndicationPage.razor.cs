using ViciOne.Ui.Blazor.Components.ComboBox;
using ViciOne.Ui.Blazor.Components.ContentLoadingIndication.Enums;

namespace Shared.Pages.ContentLoadingIndication;

public sealed partial class ContentLoadingIndicationPage
{
    private readonly List<ComboBoxItem<string?, string>> _comboBoxItems = [
        new () {Text = "Lorem ipsum dolor sit amet, ", Value = null},
        new () {Text = "Ut wisi enim ad minim veniam, quis nostrud exerci", Value = "1"},
    ];
    private string? _comboBoxSelectedItem;
    private bool _comboBoxLoadingIndicationVisible;
    private LoadingIndicationKind _comboBoxLoadingIndicationKind = LoadingIndicationKind.SpinnerRight;

    private bool _plainTextLoadingIndicationVisible;
    private LoadingIndicationKind _plainTextLoadingIndicationKind = LoadingIndicationKind.SpinnerLeft;

    private bool _noAnimatedTransitionOnFirstRenderContentLoadingIndicationVisible = true;

    private async Task ReloadComboBoxAsync(LoadingIndicationKind kind)
    {
        _comboBoxLoadingIndicationVisible = true;

        _comboBoxLoadingIndicationKind = kind;

        await Task.Delay(5000);

        _comboBoxLoadingIndicationVisible = false;
    }

    private async Task ReloadPlainTextAsync(LoadingIndicationKind kind)
    {
        _plainTextLoadingIndicationVisible = true;

        _plainTextLoadingIndicationKind = kind;

        await Task.Delay(5000);

        _plainTextLoadingIndicationVisible = false;
    }

    private void ToggleVisible()
        => _noAnimatedTransitionOnFirstRenderContentLoadingIndicationVisible =
            !_noAnimatedTransitionOnFirstRenderContentLoadingIndicationVisible;
}
