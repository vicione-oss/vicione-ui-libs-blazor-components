using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.CheckBox;

namespace Shared.Pages.CheckBox.Components;

public sealed partial class CheckBoxPage : ComponentBase
{
    private bool _value = true;
    private bool? _nullableValue;

    private CheckBox<bool>? _unboundCheckbox;
    private bool? _unboundCheckboxValue;

    private void GetUnboundCheckBoxValueClick()
        => _unboundCheckboxValue = _unboundCheckbox?.Value;
}
