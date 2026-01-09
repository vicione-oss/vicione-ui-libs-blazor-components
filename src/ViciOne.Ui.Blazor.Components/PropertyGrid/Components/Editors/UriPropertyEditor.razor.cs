using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components.Editors;

/// <summary>
/// Property editor for URI values
/// </summary>
public sealed partial class UriPropertyEditor
    : PropertyEditorBase<Uri?>, IHasAllowNull, IHasUpdateKey, IHasValueParseError, IHasEnterPressed<Uri?>, IHasEscapePressed<Uri?>
{
    private string? _valuePassed;

    private bool _valueEnteredInvalid;
    private object? _updateKey;

    /// <inheritdoc/>
    [Parameter] public bool AllowNull { get; set; }

    /// <inheritdoc/>
    [Parameter] public object? UpdateKey { get; set; }

    /// <inheritdoc/>
    [Parameter] public EventCallback<string> ValueParseError { get; set; }

    /// <inheritdoc/>
    [Parameter] public EventCallback<Uri?> EnterPressed { get; set; }

    /// <inheritdoc/>
    [Parameter] public EventCallback<Uri?> EscapePressed { get; set; }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        var value = Value?.ToString();
        if (string.CompareOrdinal(_valuePassed, value) != 0 || UpdateKey != _updateKey)
        {
            _valuePassed = value;
            _valueEnteredInvalid = false;
        }

        _updateKey = UpdateKey;
    }

    private Task TextBoxValueChangedAsync(string? value)
        => ParseValueAndRaiseValueChangedOrValueParseErrorAsync(value);

    private Task TextBoxEnterPressedAsync(string? value)
        => ParseValueAndRaiseValueChangedOrValueParseErrorAsync(value);

    private Task TextBoxEscapePressedAsync(string? value)
        => ParseValueAndRaiseValueChangedOrValueParseErrorAsync(value);

    private async Task ParseValueAndRaiseValueChangedOrValueParseErrorAsync(string? value)
    {
        try
        {
            Uri? uri;

            if (string.IsNullOrWhiteSpace(value) && AllowNull)
                uri = null;
            else
                uri = new UriBuilder(value ?? string.Empty).Uri;

            _valueEnteredInvalid = false;

            if (ValueChanged.HasDelegate)
                await ValueChanged.InvokeAsync(uri);
        }
        catch (UriFormatException ex)
        {
            _valueEnteredInvalid = true;

            if (ValueParseError.HasDelegate)
                await ValueParseError.InvokeAsync(ex.Message);
        }
    }
}
