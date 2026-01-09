using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.CheckBox.Enums;
using ViciOne.Ui.Blazor.Components.CheckBox.Services;
using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.CheckBox;

/// <summary>
/// Component that allows users to toggle between two states
/// </summary>
public sealed partial class CheckBox<TValue> : ComponentBase, IFocusable, ICheckBox<TValue>, IHasValidFlag
{
    private CheckBoxMode _mode;
    private string? _inputValue;
    private ElementReference _inputElementReference;

    /// <summary>
    /// True when user input should be allowed, otherwise false
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Value of the check-box
    /// </summary>
    [Parameter]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// Raised when <see cref="Value" /> has changed
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// Specifies a lambda expression that identifies the <see cref="Value"/> property’s bound value
    /// when the component is placed in the EditForm﻿
    /// </summary>
    [Parameter]
    public Expression<Func<TValue>>? ValueExpression { get; set; }

    /// <summary>
    /// Specifies the value that corresponds to the checked state
    /// </summary>
    [Parameter]
    public TValue ValueChecked { get; set; } = default!;

    /// <summary>
    /// Specifies the value that corresponds to the indeterminate state
    /// </summary>
    [Parameter]
    public TValue ValueIndeterminate { get; set; } = default!;

    /// <summary>
    /// Specifies the value that corresponds to the unchecked state
    /// </summary>
    [Parameter]
    public TValue ValueUnchecked { get; set; } = default!;

    /// <summary>
    /// Specifies whether the check-box supports the indeterminate state
    /// </summary>
    [Parameter]
    public bool AllowIndeterminateState { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public bool? Valid { get; set; }

#pragma warning disable IDE0051 // Remove unused private members
    /// <summary>
    /// Inject target for an instance that applies parameter defaults
    /// </summary>
    [Inject]
    private ICheckBoxParameterDefaults<TValue> CheckBoxParameterDefaults
    {
        set => value.Apply(this);
    }
#pragma warning restore IDE0051 // Remove unused private members


    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        _mode = GetCheckBoxMode();
        _inputValue = GetInputValue();
    }

    private CheckBoxMode GetCheckBoxMode()
    {
        if (AllowIndeterminateState && Value?.ToString() == ValueIndeterminate?.ToString())
            return CheckBoxMode.Indeterminate;

        if (Value?.ToString() == ValueChecked?.ToString())
            return CheckBoxMode.Checked;
        else
            return CheckBoxMode.Unchecked;
    }

    private string? GetInputValue()
    {
        TValue value;

        if (_mode == CheckBoxMode.Indeterminate)
            value = ValueIndeterminate;
        else if (_mode == CheckBoxMode.Checked)
            value = ValueChecked;
        else
            value = ValueUnchecked;

        return value is null ? "null" : $"{value}";
    }

    private async Task InputChangeAsync(ChangeEventArgs _)
    {
        // Indeterminate → Checked → Unchecked → Indeterminate

        if (_mode == CheckBoxMode.Unchecked)
        {
            if (AllowIndeterminateState)
                await HandleValueChangedAsync(ValueIndeterminate);
            else
                await HandleValueChangedAsync(ValueChecked);
        }
        else if (_mode == CheckBoxMode.Checked)
        {
            await HandleValueChangedAsync(ValueUnchecked);
        }
        else if (_mode == CheckBoxMode.Indeterminate)
        {
            await HandleValueChangedAsync(ValueChecked);
        }
    }

    private async Task HandleValueChangedAsync(TValue value)
    {
        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(value);

        Value = value;

        _mode = GetCheckBoxMode();
        _inputValue = GetInputValue();
    }

    /// <inheritdoc/>
    public async Task FocusAsync()
        => await _inputElementReference.FocusAsync();
}
