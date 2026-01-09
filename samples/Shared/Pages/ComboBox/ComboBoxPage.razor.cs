using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Button.Enums;
using ViciOne.Ui.Blazor.Components.ComboBox;
using ViciOne.Ui.Blazor.Components.Factories;

namespace Shared.Pages.ComboBox;

public sealed partial class ComboBoxPage : ComponentBase
{
    private ButtonSize _buttonSize = ButtonSize.Medium;

    private readonly List<ComboBoxItem<ButtonSize, string>> _buttonSizeComboBoxItems = [..
        TypeSafeEnumFactory<ButtonSize>.CreateAll().Select(buttonSize => new ComboBoxItem<ButtonSize, string>
        {
            Value = buttonSize,
            Text = buttonSize.GetName(),
        })
    ];

    private readonly List<ButtonSize> _simpleItems = [.. TypeSafeEnumFactory<ButtonSize>.CreateAll()];

    private void ResetSelectionButtonClick()
        => _buttonSize = ButtonSize.GetDefaultValue();
}
