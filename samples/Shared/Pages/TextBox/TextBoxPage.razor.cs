using Microsoft.AspNetCore.Components;
using TextBoxComponent = ViciOne.Ui.Blazor.Components.TextBox.TextBox;

namespace Shared.Pages.TextBox;

public sealed partial class TextBoxPage : ComponentBase
{
    private string _textBoxBoundValue = "Lorem ipsum";
    private string? _textBoxInputValue1 = "Lorem ipsum";
    private string? _textBoxInputValue2 = "Lorem ipsum";
    private string? _textBoxInputValue3 = "Lorem ipsum";
    private string? _textBoxInputValue4 = "Lorem ipsum";
    private int _textBoxInputValueChangingCount;
    private int _textBoxInputValueChangedCount;

    private TextBoxComponent? _unboundTextBox;
    private string? _unboundTextBoxValue;

    private void GetUnboundTextBoxValueClick()
        => _unboundTextBoxValue = _unboundTextBox?.Value;

    private void TextBoxValueChanging(string value)
    {
        _textBoxInputValue1 = value;
        _textBoxInputValueChangingCount++;
    }

    private void TextBoxValueChanged(string value)
    {
        _textBoxInputValue4 = value;
        _textBoxInputValueChangedCount++;
    }
}
