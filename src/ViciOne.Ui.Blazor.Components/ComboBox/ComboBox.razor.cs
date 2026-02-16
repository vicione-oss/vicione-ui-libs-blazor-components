using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
        public required Dictionary<string, object> Attributes { get; set; }
        public required string Text { get; init; }
        public required TValue? Value { get; init; }
        public bool ValueAttributeHasGuid { get; set; }
    }

    private bool _firstParameterSet = true;
    private readonly string _nullValue = "null";
    private TValue? _value;
    private IEnumerable<TItem> _items = [];
    private readonly List<OptionDescriptor> _optionDescriptors = [];
    private readonly Dictionary<object, OptionDescriptor> _optionDescriptorMap = [];
    private string _selectElementKey = Guid.NewGuid().ToString();
    private ElementReference _selectElementReference;
    private bool _focused;
    private bool _restoreFocus;
    private bool _isAnyOptionSelected;
    private Expression<Func<TItem, TValue>>? _valueSelector;
    private Expression<Func<TItem, string>>? _textSelector;
    private Func<TItem, TValue>? _getValueFunc;
    private Func<TItem, string>? _getTextFunc;
    private object? _updateKey;

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
            _optionDescriptorMap.Clear();
            _isAnyOptionSelected = false;

            var useGuidKeys = false;

            foreach (var item in Items)
            {
                var optionValueStr = $"{item}";
                var optionValueStrContainsGuid = false;
                var optionValueTyped = item is TValue itemTyped ? itemTyped : default;

                var optionText = optionValueStr;

                var optionSelected = false;

                if (_getValueFunc is not null)
                {
                    var value = _getValueFunc.Invoke(item);
                    if (value is null)
                    {
                        optionValueStr = _nullValue;
                        optionValueTyped = default;
                    }
                    else if (value is TValue valueTyped)
                    {
                        optionValueStr = $"{valueTyped}";
                        optionValueTyped = valueTyped;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Value could not be handled for {item}");
                    }
                }

                if (_getTextFunc?.Invoke(item) is string str)
                    optionText = str;

                if (NoOptionSelected is not true && valueEqualityComparer.Equals(optionValueTyped, Value))
                    optionSelected = true;

                if (_optionDescriptorMap.ContainsKey(optionValueStr))
                {
                    optionValueStr = Guid.NewGuid().ToString(); // switch to GUID as value is not uniquely stringified

                    optionValueStrContainsGuid = true;
                    useGuidKeys = true;
                }

                var optionAttributes = new Dictionary<string, object> { { "value", optionValueStr } };

                if (optionSelected)
                {
                    optionAttributes.Add("selected", "selected");

                    _isAnyOptionSelected = true;
                }

                var optionDescriptor = new OptionDescriptor
                {
                    Text = optionText,
                    Attributes = optionAttributes,
                    Value = optionValueTyped,
                    ValueAttributeHasGuid = optionValueStrContainsGuid
                };

                _optionDescriptors.Add(optionDescriptor);
                _optionDescriptorMap.Add(optionValueStr, optionDescriptor);
            }

            // If we use GUIDs as keys, ensure all keys are actually GUIDs and replace where not
            if (useGuidKeys)
            {
                var pairs = _optionDescriptorMap.Where(p => !p.Value.ValueAttributeHasGuid).ToList();

                foreach (var pair in pairs)
                {
                    var oldKey = pair.Key;
                    var optionDescriptor = pair.Value;

                    var newKey = Guid.NewGuid().ToString();

                    optionDescriptor.Attributes["value"] = newKey;
                    optionDescriptor.ValueAttributeHasGuid = true;

                    _optionDescriptorMap.Remove(oldKey);
                    _optionDescriptorMap.Add(newKey, optionDescriptor);
                }
            }

            _items = Items;
            _value = Value;

            // Generate new key for select element to enforce replacing the whole element,
            // otherwise browser could get confused due to dynamic re-rendering of options ...
            _selectElementKey = Guid.NewGuid().ToString();

            // ... and because of forced re-render we need to restore our focus
            _restoreFocus = _focused;
        }

        _firstParameterSet = false;
        _updateKey = UpdateKey;
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_restoreFocus)
        {
            await _selectElementReference.FocusAsync();

            _restoreFocus = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task SelectChangedAsync(ChangeEventArgs args)
    {
        // Handle selection of "null"
        var hasNullableValueType = GenericParameterHelper.IsNullable<TValue>();

        if (args.Value is null)
        {
            if (ValueChanged.HasDelegate && hasNullableValueType)
                await ValueChanged.InvokeAsync(default);

            return;
        }

        if (args.Value is string s && s == _nullValue)
        {
            if (ValueChanged.HasDelegate && hasNullableValueType)
                await ValueChanged.InvokeAsync(default);

            return;
        }

        // Handle selection for non-nullable TValue
        if (_optionDescriptorMap.TryGetValue(args.Value, out var optionDescriptor))
        {
            if (optionDescriptor.Value is null && !hasNullableValueType)
                return;

            if (ValueChanged.HasDelegate)
                await ValueChanged.InvokeAsync(optionDescriptor.Value);
        }
    }

    private void SelectFocus(FocusEventArgs _)
        => _focused = true;

    private void SelectBlur(FocusEventArgs _)
        => _focused = false;

    /// <inheritdoc/>
    public async Task FocusAsync()
        => await _selectElementReference.FocusAsync();

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

        if (!newItems.TryGetNonEnumeratedCount(out var newItemsCount))
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
