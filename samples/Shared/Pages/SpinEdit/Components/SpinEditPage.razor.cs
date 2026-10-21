using Bogus;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Button.Enums;
using ViciOne.Ui.Blazor.Components.Factories;
using ViciOne.Ui.Blazor.Components.SpinEdit;

namespace Shared.Pages.SpinEdit.Components;

public sealed partial class SpinEditPage : ComponentBase
{
    private static readonly Faker s_faker = new();

    private int _value = 120;
    private int? _nullableValue;
    private ButtonSize _buttonSize = ButtonSize.GetDefaultValue();
    private readonly ButtonSize _buttonSizeFirst = TypeSafeEnumFactory<ButtonSize>.CreateAll().First();
    private readonly ButtonSize _buttonSizeLast = TypeSafeEnumFactory<ButtonSize>.CreateAll().Last();
    private bool _isRastered = true;

    private SpinEdit<int, int, int>? _unboundSpinEdit;
    private int _unboundSpinEditValue = 150;

    // Enough sentences that even the shortest ones overflow the scrollable container, which the end-to-end
    // tests rely on to have room to scroll; a single paragraph can be as short as one sentence
    private readonly string _lorem1 = s_faker.Lorem.Sentences(20, " ");
    private readonly string _lorem2 = s_faker.Lorem.Sentences(20, " ");

    private void GetUnboundSpinEditValueClick()
    {
        if (_unboundSpinEdit is null)
            return;

        _unboundSpinEditValue = _unboundSpinEdit.Value;
    }
}
