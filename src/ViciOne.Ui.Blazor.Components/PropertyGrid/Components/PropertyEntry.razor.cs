using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Interfaces;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Components.Editors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Messages;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components;

/// <summary>
/// Represents an entry in the property grid, which renders a label, a property editor
/// and other UI elements to allow edit of underlying properties.
/// </summary>
public sealed partial class PropertyEntry<TPropertyValue> : ComponentBase, IDisposable
{
    private PropertyEntryContextMenuContext? _contextMenuContext;
    private bool _visible;
    private bool _keepMessages;
    private bool _hasWizard;
    private bool _requestHideMessages;
    private IPropertyGridItem<TPropertyValue>? _propertyGridItem;
    private bool _shouldRender;
    private bool _showInfos;
    private readonly List<InfoMessage> _infoMessages = [];
    private bool _showErrors;
    private readonly List<ErrorMessage> _errorMessages = [];
    private bool _focusRequestedFromOwnCode;
    private ValueOf<TPropertyValue>? _value;
    private IPropertyGridMessageStore? _messageStore;
    private IPropertyGridEvents? _events;
    private DynamicComponent? _dynamicComponent;
    private object? _updateKey;
    private bool _skipInputEnterOrEscapePressedCallback;

    [CascadingParameter] private IPropertyGridController Controller { get; set; } = default!;

    /// <summary>
    /// The indentation level of the property entry.
    /// </summary>
    [Parameter] public int IndentationMultiplier { get; set; }

    /// <summary>
    /// <see langword="true" /> when the property should be visible, otherwise <see langword="false" />.
    /// </summary>
    [Parameter] public bool Visible { get; set; }

    /// <inheritdoc cref="IPropertyGridState.KeepMessages"/>
    [Parameter] public bool KeepMessages { get; set; }

    /// <summary>
    /// The property grid item modeling the API used to coordinate edits of the underlying properties.
    /// </summary>
    [Parameter, EditorRequired] public required IPropertyGridItem<TPropertyValue> PropertyGridItem { get; set; }

    [Inject] private IPropertyEditorComponentRegistry PropertyEditorComponentRegistry { get; set; } = default!;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        Controller.UpdatePropertyRequested += UpdatePropertyRequestedAsync;
        Controller.FocusPropertyRequested += FocusPropertyRequestedAsync;
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (PropertyGridItem != _propertyGridItem)
        {
            _messageStore?.Changed -= MessageStoreChangedAsync;

            _propertyGridItem = PropertyGridItem;

            _messageStore = _propertyGridItem.MessageStore;
            _messageStore.Changed += MessageStoreChangedAsync;

            UpdateValue(true);

            _shouldRender = true;
        }

        if (Visible != _visible)
        {
            _visible = Visible;

            _shouldRender = true;
        }

        if (KeepMessages != _keepMessages)
        {
            _keepMessages = KeepMessages;

            _shouldRender = true;
        }

        var hasWizard = PropertyGridItem.Resettable || PropertyGridItem.CanBeSetToNull;
        if (hasWizard != _hasWizard)
        {
            _hasWizard = hasWizard;

            _shouldRender = true;
        }

        _events = Controller.Events;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _messageStore?.Changed -= MessageStoreChangedAsync;

        Controller.FocusPropertyRequested -= FocusPropertyRequestedAsync;
        Controller.UpdatePropertyRequested -= UpdatePropertyRequestedAsync;

        _contextMenuContext?.OnSetValue -= ContextMenuContexSetValue;
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
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        Interlocked.Exchange(ref _skipInputEnterOrEscapePressedCallback, false);

        if (Interlocked.CompareExchange(ref _requestHideMessages, false, true))
            await HideMessagesAsync();
    }

    private async Task FocusAsync()
    {
        if (_dynamicComponent?.Instance is PropertyEditorBase<TPropertyValue> propertyEditorComponent)
        {
            if (propertyEditorComponent.InnerEditorComponentReference is IFocusable focusable)
            {
                Interlocked.Exchange(ref _focusRequestedFromOwnCode, true);

                await focusable.FocusAsync();
            }
        }
    }

    private async void FocusPropertyRequestedAsync(PropertyGridControllerFocusPropertyRequestedEventArgs args)
    {
        if (string.Equals(args.Name, PropertyGridItem.Name, StringComparison.Ordinal))
        {
            if (_dynamicComponent?.Instance is PropertyEditorBase<TPropertyValue> propertyEditorComponent)
            {
                if (propertyEditorComponent.InnerEditorComponentReference is IFocusable focusable)
                    await focusable.FocusAsync();

                if (propertyEditorComponent.InnerEditorComponentReference is IHasSelectableContent hasSelectableContent)
                    await hasSelectableContent.SelectContentAsync();
            }
        }
    }

    private async void UpdatePropertyRequestedAsync(PropertyGridControllerUpdatePropertyRequestedEventArgs args)
    {
        if (args.Targets.Contains(PropertyGridItem))
        {
            UpdateValue(true);

            // we do not call UpdateDependents() here as event carries dependents in args.Targets

            _shouldRender = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void MessageStoreChangedAsync(PropertyGridMessageStoreChangedEventArgs args)
    {
        if (args.Added.Contains(PropertyGridItem))
        {
            var messages = args.Sender.Get(PropertyGridItem);

            _infoMessages.Clear();
            _infoMessages.AddRange(messages.OfType<InfoMessage>());
            _showInfos = _infoMessages.Count > 0;

            _errorMessages.Clear();
            _errorMessages.AddRange(messages.OfType<ErrorMessage>());
            _showErrors = _errorMessages.Count > 0;

            _shouldRender = true;
            await InvokeAsync(StateHasChanged);
        }
        else if (args.Removed.Contains(PropertyGridItem))
        {
            _infoMessages.Clear();
            _showInfos = false;

            _errorMessages.Clear();
            _showErrors = false;

            _shouldRender = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private bool HasValueDifferentFromDefaultValue()
        => PropertyGridItem.IsDefaultValueDifferentFrom(_value);

    private bool IsVisible()
        => PropertyGridItem.Visible;

    private bool HasInfoMessages()
        => _infoMessages.Count > 0;

    private bool HasErrorMessages()
        => _errorMessages.Count > 0;

    private async Task OnFocusInAsync()
    {
        Interlocked.Exchange(ref _requestHideMessages, false);

        if (Interlocked.CompareExchange(ref _focusRequestedFromOwnCode, false, true))
        {
            _focusRequestedFromOwnCode = false;
        }
        else
        {
            var somethingChanged = false;

            if (HasInfoMessages())
            {
                if (SetShowInfos(true))
                    somethingChanged = true;
            }

            if (HasErrorMessages())
            {
                if (SetShowErrors(true))
                    somethingChanged = true;
            }

            if (somethingChanged)
                await InvokeAsync(StateHasChanged);
        }
    }

    private void OnFocusOut()
    {
        if (!_keepMessages)
        {
            Interlocked.Exchange(ref _requestHideMessages, true);

            _shouldRender = true;
        }
    }

    private async Task HideMessagesAsync()
    {
        var somethingChanged = false;

        if (SetShowInfos(false))
            somethingChanged = true;

        if (SetShowErrors(false))
            somethingChanged = true;

        if (somethingChanged)
            await InvokeAsync(StateHasChanged);
    }

    private async Task OnPropertyWizardButtonClickedAsync(MouseEventArgs e)
    {
        if (_contextMenuContext is null)
        {
            _contextMenuContext = new() { PropertyGridItem = PropertyGridItem, MouseEventArgs = e };
            _contextMenuContext.OnSetValue += ContextMenuContexSetValue;
        }
        else
        {
            _contextMenuContext.PropertyGridItem = PropertyGridItem;
            _contextMenuContext.MouseEventArgs = e;
        }

        await Controller.ShowContextMenuAsync(_contextMenuContext);
    }

    private void ContextMenuContexSetValue()
    {
        UpdateValue(true);

        _events?.NotifyPropertyChanged(PropertyGridItem);

        UpdateDependents();

        _shouldRender = true;
        InvokeAsync(StateHasChanged);
    }

    private bool SetShowInfos(bool showInfos)
    {
        if (showInfos != _showInfos)
        {
            _showInfos = showInfos;
            _shouldRender = true;

            return true;
        }

        return false;
    }

    private bool SetShowErrors(bool showErrors)
    {
        if (showErrors != _showErrors)
        {
            _showErrors = showErrors;
            _shouldRender = true;

            return true;
        }

        return false;
    }

    private EventCallback<string> CreateValueParseErrorCallback()
        => EventCallback.Factory.Create<string>(this, InputValueParseError);

    private void InputValueParseError(string errorMessage)
    {
        if (_messageStore is not null)
        {
            _messageStore.Remove(PropertyGridItem);
            _messageStore.Add(PropertyGridItem, new ErrorMessage { Text = errorMessage });
        }
    }

    private object CreateEnterPressedCallback(Type valueType)
    {
        var d = CreateInputEnterOrEscapePressedCallback<object>;
        var result = d.Method.GetGenericMethodDefinition().MakeGenericMethod(valueType)
            .Invoke(this, []);

        return result!;
    }

    private object CreateEscapePressedCallback(Type valueType)
        => CreateEnterPressedCallback(valueType);

    private EventCallback<T> CreateInputEnterOrEscapePressedCallback<T>()
        => EventCallback.Factory.Create(this, (T newValue) => InputEnterOrEscapePressedAsync((TPropertyValue)(object)newValue!));

    private async Task InputEnterOrEscapePressedAsync(TPropertyValue newValue)
    {
        if (_skipInputEnterOrEscapePressedCallback)
            return;

        if (!IsValid())
            await ValidateCompareAndSetValueAsync(newValue);
    }

    private object CreateValueChangedCallback(Type valueType)
    {
        var d = CreateValueChangedCallback<object>;
        var result = d.Method.GetGenericMethodDefinition().MakeGenericMethod(valueType)
            .Invoke(this, []);

        return result!;
    }

    private EventCallback<T> CreateValueChangedCallback<T>()
        => EventCallback.Factory.Create(this, (T newValue) => InputValueChangedAsync((TPropertyValue)(object)newValue!));

    private Task InputValueChangedAsync(TPropertyValue newValue)
        => ValidateCompareAndSetValueAsync(newValue);

    private async Task ValidateCompareAndSetValueAsync(TPropertyValue newValue)
    {
        if (!PropertyGridItem.Validate(newValue))
        {
            Interlocked.Exchange(ref _skipInputEnterOrEscapePressedCallback, true);

            return;
        }

        // Compare incoming value with last value set to avoid raising PropertyChanged unnecessarily
        if (_value is not null && PropertyGridItem.ValueEqualityComparer.Equals(_value.Value, newValue))
            return;

        var success = PropertyGridItem.SetValue(newValue);

        // Read unified value again as implementations of IPropertyDescriptor.SetValue might change the value
        // to something different than passed with newValue
        _value = PropertyGridItem.ReadUnifiedValue();

        if (success)
        {
            _events?.NotifyPropertyChanged(PropertyGridItem);

            UpdateDependents();
        }

        _updateKey = new object(); // New update key because _value could have the same value as before
        _shouldRender = true;
        await InvokeAsync(StateHasChanged);
    }

    private void UpdateDependents()
        => Controller.UpdateDependents(PropertyGridItem);

    private static IEnumerable<SelectableValue<T?>> MakeSelectableValuesNullable<T>(IEnumerable<ISelectableValue<T>> selectableValues)
        where T : struct
            => [.. selectableValues.Select(v => new SelectableValue<T?> { Value = v.Value, Text = v.Text })];

    private void UpdateValue(bool withValidation)
    {
        _value = PropertyGridItem.ReadUnifiedValue();

        _messageStore?.Remove(PropertyGridItem);

        if (withValidation && IsVisible())
        {
            if (_value is not null)
                PropertyGridItem.Validate(_value.Value);
        }

        // Previously, the code here did force a refresh of the editor component via a newly generated key passed to @key
        // to circumvent optimizations in OnParameterSet of editor components. These optimizations are implemented to
        // avoid losing input state in those components.
        //
        // The @key approach resulted in a range of issues like losing focus, unecessarily freeing up / constructing
        // new editor component instances. All those issues require countermeasures making the code more complex and
        // error-prone.
        //
        // So, the @key approach has been removed and update key handling has been introduced to enforce an
        // update of the editor component when needed.

        _updateKey = new object();
    }

    private async Task OnCloseInfoMessagesAsync(params IEnumerable<InfoMessage> infoMessages)
    {
        foreach (var infoMessage in infoMessages)
            _infoMessages.Remove(infoMessage);

        if (_infoMessages.Count == 0)
        {
            if (SetShowInfos(false))
                await InvokeAsync(StateHasChanged);
        }

        await FocusAsync();
    }

    private async Task OnCloseErrorMessageAsync()
    {
        // no removal of errors as errors should persist as long as the value is invalid

        if (SetShowErrors(false))
            await InvokeAsync(StateHasChanged);

        await FocusAsync();
    }

    private bool IsValid()
        => _errorMessages.Count == 0 && _infoMessages.Count == 0;
}
