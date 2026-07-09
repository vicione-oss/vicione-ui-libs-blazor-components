using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.DropDown;
using ViciOne.Ui.Blazor.Components.Helpers;
using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.ComboBox;

/// <summary>
/// Component for rendering a combo-box UI element
/// </summary>
public sealed partial class ComboBox<TItem, TValue> : ComponentBase, IFocusable, IHasValidFlag, IHasUpdateKey
{
    private sealed class OptionDescriptor
    {
        public required string Text { get; init; }
        public required TValue? Value { get; init; }
    }

    private bool _firstParameterSet = true;
    private TValue? _value;
    private IEnumerable<TItem> _items = [];
    private readonly List<OptionDescriptor> _optionDescriptors = [];
    private OptionDescriptor? _selectedOptionDescriptor;
    private TextBox.TextBox? _textBox;
    private DropDown<OptionDescriptor>? _dropDown;
    private string _inputValue = string.Empty;
    private Expression<Func<TItem, TValue>>? _valueSelector;
    private Expression<Func<TItem, string>>? _textSelector;
    private Func<TItem, TValue>? _getValueFunc;
    private Func<TItem, string>? _getTextFunc;
    private object? _updateKey;
    private bool _dropDownVisible;
    private bool _keepDropDownClosed;
    private readonly bool _hasNullableValueType = GenericParameterHelper.IsNullable<TValue>();

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Items selectable from the drop-down of the combo-box
    /// </summary>
    [Parameter, EditorRequired]
    public required IEnumerable<TItem> Items { get; set; }

    /// <summary>
    /// <see cref="ValueSelector">Value</see> of the selected <typeparamref name="TItem"/>
    /// </summary>
    [Parameter, EditorRequired]
    public required TValue Value { get; set; }

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
    /// Selector for the value property of <typeparamref name="TItem"/>
    /// </summary>
    [Parameter]
    public Expression<Func<TItem, TValue>>? ValueSelector { get; set; }

    /// <summary>
    /// Selector for the text property of <typeparamref name="TItem"/>
    /// </summary>
    [Parameter]
    public Expression<Func<TItem, string>>? TextSelector { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public bool? Valid { get; set; }

    /// <summary>
    /// True when the combo-box should be read-only, otherwise false
    /// </summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// True when user can add custom values, otherwise false
    /// </summary>
    [Parameter]
    public bool AllowUserInput { get; set; }

    /// <summary>
    /// True when user input should be allowed, otherwise false
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// True when no option should be selected no matter the state of <see cref="Value"/>, otherwise false or null.
    /// </summary>
    [Parameter]
    public bool? NoOptionSelected { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public object? UpdateKey { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        var valueEqualityComparer = EqualityComparer<TValue>.Default;

        if (ValueSelector != _valueSelector)
        {
            _valueSelector = ValueSelector;

            _getValueFunc = _valueSelector?.Compile();
        }

        if (TextSelector != _textSelector)
        {
            _textSelector = TextSelector;

            _getTextFunc = _textSelector?.Compile();
        }

        if (_firstParameterSet ||
            IsItemsChanged(_items, Items, valueEqualityComparer, _getValueFunc, _getTextFunc) ||
            !valueEqualityComparer.Equals(Value, _value) ||
            UpdateKey != _updateKey)
        {
            _optionDescriptors.Clear();
            _selectedOptionDescriptor = null;

            foreach (var item in Items)
            {
                var optionText = $"{item}";
                var optionValueTyped = item is TValue itemTyped ? itemTyped : default;

                if (_getValueFunc is not null)
                {
                    var value = _getValueFunc.Invoke(item);
                    if (value is null)
                    {
                        optionValueTyped = default;
                    }
                    else if (value is TValue valueTyped)
                    {
                        optionValueTyped = valueTyped;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Value could not be handled for {item}");
                    }
                }

                if (_getTextFunc?.Invoke(item) is string str)
                    optionText = str;

                var optionDescriptor = new OptionDescriptor
                {
                    Text = optionText,
                    Value = optionValueTyped
                };

                _optionDescriptors.Add(optionDescriptor);

                if (NoOptionSelected is not true &&
                    _selectedOptionDescriptor is null &&
                    valueEqualityComparer.Equals(optionValueTyped, Value))
                {
                    _selectedOptionDescriptor = optionDescriptor;
                }
            }

            _items = Items;
            _value = Value;

            _inputValue = _selectedOptionDescriptor?.Text
                ?? (AllowUserInput && Value is not null ? $"{Value}" : string.Empty);
        }

        _firstParameterSet = false;
        _updateKey = UpdateKey;
    }

    /// <inheritdoc />
    protected override void OnAfterRender(bool firstRender)
    {
        // The compiled TypeScript runs after the first render, so the width and height values are not set yet. Re-rendering here applies
        // those values and prevents jumping effect of the ComboBox.
        if (firstRender)
            StateHasChanged();
    }

    private IReadOnlyCollection<OptionDescriptor> GetSelectedOptionDescriptors()
    {
        if (_selectedOptionDescriptor is null)
            return [];

        return [_selectedOptionDescriptor];
    }

    private List<OptionDescriptor> GetFilteredOptionDescriptors()
    {
        if (string.IsNullOrEmpty(_inputValue))
            return _optionDescriptors;

        var selectedText = _selectedOptionDescriptor?.Text ?? $"{Value}";

        if (selectedText == _inputValue)
            return _optionDescriptors;

        return [.. _optionDescriptors.Where(descriptor => descriptor.Text.Contains(_inputValue, StringComparison.OrdinalIgnoreCase))];
    }

    private async Task OpenDropdownAsync()
    {
        _keepDropDownClosed = false;

        if (_dropDown is not null && Enabled && !ReadOnly)
        {
            await _dropDown.ShowAsync();

            _dropDownVisible = true;
        }
    }

    private async Task CloseDropdownAsync()
    {
        await SetUserInputAsync();

        if (_dropDown is not null)
        {
            await _dropDown.HideAsync();

            _dropDownVisible = false;
        }
    }

    private async Task OpenOrCloseDropdownAsync()
    {
        if (_dropDownVisible)
            await CloseDropdownAsync();
        else
            await OpenDropdownAsync();
    }

    private async Task InputValueChangingAsync(string? value)
    {
        _inputValue = value ?? string.Empty;

        if (!_dropDownVisible)
            await OpenDropdownAsync();
    }

    private async Task InputEnterPressedAsync(string? _)
    {
        if (_keepDropDownClosed)
        {
            _keepDropDownClosed = false;

            return;
        }

        if (!_dropDownVisible)
        {
            await OpenDropdownAsync();

            return;
        }

        await SetUserInputAsync();
    }

    private async Task SetUserInputAsync()
    {
        if (!AllowUserInput)
        {
            _inputValue = _selectedOptionDescriptor?.Text ?? string.Empty;

            return;
        }

        var matchedDescriptor = _optionDescriptors.FirstOrDefault(
            d => string.Equals(d.Text, _inputValue, StringComparison.OrdinalIgnoreCase));

        if (matchedDescriptor is not null)
        {
            if (matchedDescriptor.Value is null && !_hasNullableValueType)
                return;

            if (!EqualityComparer<TValue>.Default.Equals(matchedDescriptor.Value, _value) && ValueChanged.HasDelegate)
                await ValueChanged.InvokeAsync(matchedDescriptor.Value);

            return;
        }

        if (TryConvertToValue(_inputValue, out var value) &&
            !EqualityComparer<TValue>.Default.Equals(value, _value) &&
            ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(value);
        }
    }

    private static bool TryConvertToValue(string text, out TValue? value)
    {
        if (typeof(TValue) == typeof(string))
        {
            value = (TValue)(object)text;

            return true;
        }

        try
        {
            var underlyingType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

            value = (TValue)Convert.ChangeType(text, underlyingType, CultureInfo.CurrentCulture);

            return true;
        }
        catch
        {
            value = default;

            return false;
        }
    }

    private async Task DropdownSelectedItemsChangedAsync(IEnumerable<OptionDescriptor> selectedDescriptors)
    {
        if (selectedDescriptors.FirstOrDefault() is not { } selectedDescriptor)
            return;

        if (selectedDescriptor.Value is null && !_hasNullableValueType)
            return;

        _inputValue = selectedDescriptor.Text;

        if (_dropDown is not null)
        {
            await _dropDown.HideAsync();

            _dropDownVisible = false;
            _keepDropDownClosed = true;
        }

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(selectedDescriptor.Value);
    }

    /// <inheritdoc/>
    public async Task FocusAsync()
    {
        if (_textBox is not null)
            await _textBox.FocusAsync();
    }

    private static bool IsItemsChanged(IEnumerable<TItem> oldItems, IEnumerable<TItem> newItems,
        EqualityComparer<TValue> valueEqualityComparer, Func<TItem, TValue>? getValue,
        Func<TItem, string>? getText)
    {
        IEnumerable<TItem> oldItemsEnumerable;
        IEnumerable<TItem> newItemsEnumerable;

        if (oldItems.TryGetNonEnumeratedCount(out var oldItemsCount))
        {
            oldItemsEnumerable = oldItems;
        }
        else
        {
            List<TItem> oldItemsList = [.. oldItems];
            oldItemsCount = oldItemsList.Count;
            oldItemsEnumerable = oldItemsList;
        }

        if (newItems.TryGetNonEnumeratedCount(out var newItemsCount))
        {
            newItemsEnumerable = newItems;
        }
        else
        {
            List<TItem> newItemsList = [.. newItems];
            newItemsCount = newItemsList.Count;
            newItemsEnumerable = newItemsList;
        }

        if (newItemsCount != oldItemsCount)
            return true;

        var newItemsMap = newItemsEnumerable.Select((newItem, index) => new { NewItem = newItem, Index = index })
            .ToDictionary(keySelector: a => a.Index, elementSelector: a => a.NewItem);

        if (getValue is not null && getText is not null)
        {
            foreach (var oldItem in oldItemsEnumerable)
            {
                var oldItemValue = getValue(oldItem);
                var oldItemText = getText(oldItem);

                foreach (var p in newItemsMap)
                {
                    var newItemIndex = p.Key;
                    var newItem = p.Value;
                    var newItemValue = getValue(newItem);
                    var newItemText = getText(newItem);

                    if (valueEqualityComparer.Equals(newItemValue, oldItemValue) &&
                        string.Equals(newItemText, oldItemText, StringComparison.Ordinal))
                    {
                        newItemsMap.Remove(newItemIndex);

                        break;
                    }

                    return true;
                }
            }
        }
        else
        {
            var itemEqualityComparer = EqualityComparer<TItem>.Default;

            foreach (var oldItem in oldItemsEnumerable)
            {
                foreach (var p in newItemsMap)
                {
                    var newItem = p.Value;

                    if (itemEqualityComparer.Equals(newItem, oldItem))
                    {
                        newItemsMap.Remove(p.Key);

                        break;
                    }

                    return true;
                }
            }
        }

        return false;
    }
}
