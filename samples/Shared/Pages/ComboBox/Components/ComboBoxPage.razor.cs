using Microsoft.AspNetCore.Components;
using Shared.Pages.ComboBox.Models;
using ViciOne.Ui.Blazor.Components.Button.Enums;
using ViciOne.Ui.Blazor.Components.ComboBox;
using ViciOne.Ui.Blazor.Components.Factories;

namespace Shared.Pages.ComboBox.Components;

public sealed partial class ComboBoxPage : ComponentBase
{
    private ButtonSize _buttonSize = ButtonSize.Medium;
    private string _city = "Berlin";
    private SampleObject? _selectedSampleObject;

    private readonly List<ComboBoxItem<ButtonSize, string>> _buttonSizeComboBoxItems = [..
        TypeSafeEnumFactory<ButtonSize>.CreateAll().Select(buttonSize => new ComboBoxItem<ButtonSize, string>
        {
            Value = buttonSize,
            Text = buttonSize.GetName(),
        })
    ];

    private readonly List<string> _customInputComboBoxItems = [
        "Berlin",
        "Zurich",
        "Paris",
        "Rome"
    ];

    private readonly List<ButtonSize> _simpleItems = [.. TypeSafeEnumFactory<ButtonSize>.CreateAll()];

    private readonly List<SampleObject> _sampleObjects = [..
        Enumerable.Range(1, 100).Select(i => new SampleObject
        {
            Name = $"Sample Object {i}",
            Value = i
        })
    ];

    private void ResetSelectionButtonClick()
        => _buttonSize = ButtonSize.GetDefaultValue();

    private void ResetCitySelectionButtonClick()
        => _city = _customInputComboBoxItems[0];
}
