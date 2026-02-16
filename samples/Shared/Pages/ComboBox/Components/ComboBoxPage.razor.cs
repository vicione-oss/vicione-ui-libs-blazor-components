using Microsoft.AspNetCore.Components;
using Shared.Pages.ComboBox.Models;
using ViciOne.Ui.Blazor.Components.Button.Enums;
using ViciOne.Ui.Blazor.Components.ComboBox;
using ViciOne.Ui.Blazor.Components.Factories;

namespace Shared.Pages.ComboBox.Components;

public sealed partial class ComboBoxPage : ComponentBase
{
    private ButtonSize _buttonSize = ButtonSize.Medium;
    private SampleObject? _selectedSampleObject;

    private readonly List<ComboBoxItem<ButtonSize, string>> _buttonSizeComboBoxItems = [..
        TypeSafeEnumFactory<ButtonSize>.CreateAll().Select(buttonSize => new ComboBoxItem<ButtonSize, string>
        {
            Value = buttonSize,
            Text = buttonSize.GetName(),
        })
    ];

    private readonly List<ButtonSize> _simpleItems = [.. TypeSafeEnumFactory<ButtonSize>.CreateAll()];

    private readonly List<SampleObject> _sampleObjects = [
        new()
        {
            Name = "First",
            Value = 1
        },
        new()
        {
            Name = "Second",
            Value = 2
        },
        new()
        {
            Name = "Third",
            Value = 3
        }
    ];

    private void ResetSelectionButtonClick()
        => _buttonSize = ButtonSize.GetDefaultValue();
}
