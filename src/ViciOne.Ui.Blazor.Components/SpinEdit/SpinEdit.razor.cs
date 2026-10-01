using System.Linq.Expressions;
using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Helpers;
using ViciOne.Ui.Blazor.Components.Interfaces;
using ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;
using TextBoxComponent = ViciOne.Ui.Blazor.Components.TextBox.TextBox;

namespace ViciOne.Ui.Blazor.Components.SpinEdit;

/// <summary>
/// An editor with spin buttons that allow users to increment / decrement numeric values.
/// </summary>
public sealed partial class SpinEdit<TValue, TInterval, TLimit>
    : ComponentBase, IFocusable, IHasValidFlag, IHasUpdateKey, IDisposable
{
    private static readonly EqualityComparer<TValue> s_equalityComparer = EqualityComparer<TValue>.Default;

    private bool _firstParameterSet = true;

    private TValue _valuePassed = default!;
    private TValue _valueApplicable = default!;

    private string? _textBoxValue;

    private bool _focused;
    private ISpinBehavior<TValue, TInterval, TLimit>? _behavior;
    private bool _shouldRender;
    private object? _updateKey;

    private readonly System.Timers.Timer _spinTimer = new() { AutoReset = true, Interval = 500 };

    private TextBoxComponent? _textBox;

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Value entered in the input
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
    /// Raised when Enter was pressed in the input element, after <see cref="ValueChanged"/> has committed the value.
    /// </summary>
    [Parameter]
    public EventCallback<TValue> EnterPressed { get; set; }

    /// <summary>
    /// Interval of the increment / decrement
    /// </summary>
    [Parameter, EditorRequired]
    public required TInterval Interval { get; set; }

    /// <summary>
    /// True when only a multiple of <see cref="Interval"/> is allowed as input, otherwise false
    /// </summary>
    [Parameter]
    public bool IsRastered { get; set; }

    /// <summary>
    /// Minimum allowed as input
    /// </summary>
    [Parameter, EditorRequired]
    public required TLimit Minimum { get; set; }

    /// <summary>
    /// Maximum allowed as input
    /// </summary>
    [Parameter, EditorRequired]
    public required TLimit Maximum { get; set; }

    /// <summary>
    /// Overrides <see cref="DefaultSpinBehavior"/>
    /// </summary>
    [Parameter]
    public ISpinBehavior<TValue, TInterval, TLimit>? SpinBehavior { get; set; }

    /// <summary>
    /// Default behavior used for handling spin actions
    /// </summary>
    [Inject]
    public ISpinBehavior<TValue, TInterval, TLimit> DefaultSpinBehavior { get; set; } = default!;

    /// <summary>
    /// True when input should be read-only, otherwise false
    /// </summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// True when user input should be allowed, otherwise false
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <inheritdoc/>
    [Parameter]
    public bool? Valid { get; set; }

    /// <summary>
    /// Text rendered as <see href="https://html.spec.whatwg.org/#attr-input-placeholder">placeholder</see> when <see cref="Value"/> is null or empty
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public object? UpdateKey { get; set; }

    private ISpinBehavior<TValue, TInterval, TLimit> Behavior => _behavior ??= SpinBehavior ?? DefaultSpinBehavior;

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (!s_equalityComparer.Equals(_valuePassed, Value) || UpdateKey != _updateKey || _firstParameterSet)
        {
            _valuePassed = Value;
            _valueApplicable = _valuePassed;

            SetTextBoxValue(_valuePassed);
        }

        _shouldRender = true;
        _firstParameterSet = false;
        _updateKey = UpdateKey;
    }

    /// <inheritdoc/>
    protected override bool ShouldRender()
    {
        if (_shouldRender)
        {
            _shouldRender = false;
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _spinTimer.Stop();
        _spinTimer.Elapsed -= SpinUpTimerElapsed;
        _spinTimer.Elapsed -= SpinDownTimerElapsed;
        _spinTimer.Dispose();
    }

    private void TextBoxValueChanging(string? value)
    {
        var isNullInput = string.IsNullOrWhiteSpace(value);

        if (isNullInput)
        {
            if (GenericParameterHelper.IsNullable<TValue>())
            {
                _valueApplicable = default!;
            }
            else
            {
                // Do nothing as input is invalid and to give user the chance to change input,
                // invalid input will be reverted on enter / esc / blur
            }
        }
        else
        {
            if (Behavior.TryParse(value!, out var valueTyped))
                _valueApplicable = valueTyped;
        }
    }

    private async Task TextBoxEnterPressedAsync(string? value)
    {
        await TextBoxValueChangedAsync(value);

        if (EnterPressed.HasDelegate)
            await EnterPressed.InvokeAsync(_valueApplicable);
    }

    private async Task TextBoxEscapePressedAsync(string? value)
        => await TextBoxValueChangedAsync(value);

    private async Task TextBoxFocusLostAsync(string? value)
        => await TextBoxValueChangedAsync(value);

    private async Task TextBoxValueChangedAsync(string? value)
    {
        TextBoxValueChanging(value);

        _valueApplicable = EnsureRangeAndRaster(_valueApplicable);

        await HandleValueChangedAsync(_valueApplicable);
    }

    private bool CanReceiveFocus()
        => !ReadOnly && Enabled;

    private bool CanReceiveInput(bool mustHaveFocus)
        => !ReadOnly && Enabled && ((mustHaveFocus && _focused) || !mustHaveFocus);

    private void FocusIn(FocusEventArgs _)
    {
        if (CanReceiveFocus())
        {
            _focused = true;
            _shouldRender = true;
        }
    }

    private void FocusOut(FocusEventArgs _)
    {
        if (_focused)
        {
            _focused = false;
            _shouldRender = true;
        }
    }

    private TValue EnsureRangeAndRaster(TValue value)
    {
        var result = Behavior.EnsureRange(value, Minimum, Maximum);

        if (IsRastered)
            result = Behavior.EnsureRaster(result, Interval);

        return result;
    }

    private void KeyDown(KeyboardEventArgs args)
    {
        if (!CanReceiveInput(true))
            return;

        if (args.Code == "ArrowUp")
        {
            _valueApplicable = EnsureRangeAndRaster(_valueApplicable);
            _valueApplicable = IncrementValue(_valueApplicable);
            SetTextBoxValue(_valueApplicable);

            _spinTimer.Elapsed -= SpinDownTimerElapsed;
            _spinTimer.Elapsed -= SpinUpTimerElapsed;
            _spinTimer.Elapsed += SpinUpTimerElapsed;
            _spinTimer.Start();
        }
        else if (args.Code == "ArrowDown")
        {
            _valueApplicable = EnsureRangeAndRaster(_valueApplicable);
            _valueApplicable = DecrementValue(_valueApplicable);
            SetTextBoxValue(_valueApplicable);

            _spinTimer.Elapsed -= SpinUpTimerElapsed;
            _spinTimer.Elapsed -= SpinDownTimerElapsed;
            _spinTimer.Elapsed += SpinDownTimerElapsed;
            _spinTimer.Start();
        }
    }

    private void SpinUpTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _valueApplicable = IncrementValue(_valueApplicable);
        SetTextBoxValue(_valueApplicable);

        InvokeAsync(StateHasChanged);
    }

    private void SpinDownTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _valueApplicable = DecrementValue(_valueApplicable);
        SetTextBoxValue(_valueApplicable);

        InvokeAsync(StateHasChanged);
    }

    private async Task KeyUpAsync(KeyboardEventArgs args)
    {
        if (args.Code is "ArrowUp" or "ArrowDown")
        {
            _spinTimer.Stop();

            await HandleValueChangedAsync(_valueApplicable);
        }
    }

    private async Task WheelAsync(WheelEventArgs args)
    {
        if (!CanReceiveInput(false))
            return;

        if (!_focused)
        {
            if (_textBox is not null)
                await _textBox.FocusAsync();
        }

        if (!_focused)
            return;

        if (args.DeltaY < 0)
        {
            _valueApplicable = EnsureRangeAndRaster(_valueApplicable);
            _valueApplicable = IncrementValue(_valueApplicable);

            await HandleValueChangedAsync(_valueApplicable);
        }
        else if (args.DeltaY > 0)
        {
            _valueApplicable = EnsureRangeAndRaster(_valueApplicable);
            _valueApplicable = DecrementValue(_valueApplicable);

            await HandleValueChangedAsync(_valueApplicable);
        }
    }

    private async Task IncrementButtonClickAsync(MouseEventArgs _)
    {
        _valueApplicable = IncrementValue(_valueApplicable);

        await HandleValueChangedAsync(_valueApplicable);

        if (_textBox is not null)
            await _textBox.FocusAsync();
    }

    private async Task DecrementButtonClickAsync(MouseEventArgs _)
    {
        _valueApplicable = DecrementValue(_valueApplicable);

        await HandleValueChangedAsync(_valueApplicable);

        if (_textBox is not null)
            await _textBox.FocusAsync();
    }

    private TValue IncrementValue(TValue value)
        => Behavior.Increment(value, Interval, Minimum, Maximum);

    private TValue DecrementValue(TValue value)
        => Behavior.Decrement(value, Interval, Minimum, Maximum);

    private async Task HandleValueChangedAsync(TValue value)
    {
        SetTextBoxValue(value);

        if (s_equalityComparer.Equals(_valuePassed, value))
            return; // Early return to avoid raising event / re-render when value itself did not change actually

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(value);

        Value = value;
    }

    private void SetTextBoxValue(TValue value)
    {
        _textBoxValue = value?.ToString();

        _shouldRender = true;
        _updateKey = new object();
    }

    /// <inheritdoc/>
    public async Task FocusAsync()
    {
        if (_textBox is not null)
            await _textBox.FocusAsync();
    }
}
