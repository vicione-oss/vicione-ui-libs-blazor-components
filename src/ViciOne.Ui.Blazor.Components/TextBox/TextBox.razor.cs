using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.TextBox;

/// <summary>
/// Component for rendering a text input element
/// </summary>
public sealed partial class TextBox
    : ComponentBase, IFocusable, IHasSelectableContent, IHasValidFlag, IHasUpdateKey, IAsyncDisposable
{
    private IJSObjectReference? _jsModule;
    private IJSObjectReference? _jsAttachResult;

    private bool _disposedAsync;

    private ElementReference _inputElementReference;

    private string? _valuePassed;
    private string? _valueEntered;
    private bool _shouldRender;
    private bool _selectContentAfterRender;
    private object? _updateKey;

    /// <summary>
    /// Text rendered as <see href="https://html.spec.whatwg.org/#attr-input-placeholder">placeholder</see> when <see cref="Value"/> is null or empty
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Text entered in the input element
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Raised when the user enters something into the input
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanging { get; set; }

    /// <summary>
    /// Raised when the <see cref="Value" /> has changed and either Enter
    /// was pressed or the input loses focus
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Specifies a lambda expression that identifies the <see cref="Value"/> property’s bound value
    /// when the component is placed in the EditForm﻿
    /// </summary>
    [Parameter]
    public Expression<Func<string?>>? ValueExpression { get; set; }

    /// <summary>
    /// Raised when Enter was pressed
    /// </summary>
    [Parameter]
    public EventCallback<string?> EnterPressed { get; set; }

    /// <summary>
    /// Raised when Escape was pressed
    /// </summary>
    [Parameter]
    public EventCallback<string?> EscapePressed { get; set; }

    /// <summary>
    /// Raised when the input has lost focus
    /// </summary>
    [Parameter]
    public EventCallback<string?> FocusLost { get; set; }

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

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

    /// <summary>
    /// True when user input should be obscured so that it cannot be read, otherwise false
    /// </summary>
    /// <remarks>
    /// https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/password
    /// </remarks>
    [Parameter]
    public bool Password { get; set; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/inputmode
    /// </summary>
    [Parameter]
    public string? InputMode { get; set; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/HTML/Attributes/autocomplete
    /// </summary>
    [Parameter]
    public string? AutoComplete { get; set; }

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#naming-form-controls:-the-name-attribute">name</see> attribute
    /// of the underlying <see href="https://html.spec.whatwg.org/#the-input-element">input</see> element.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public bool? Valid { get; set; }

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#the-id-attribute">id</see> attribute
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes/spellcheck
    /// </summary>
    [Parameter]
    public bool? SpellCheck { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public object? UpdateKey { get; set; }

    [Inject] private ILogger<TextBox> Logger { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await RemoveJsAsync();

        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
            }
            _jsModule = null;
        }
    }

    private async Task<IJSObjectReference?> AttachJsAsync()
    {
        if (_jsAttachResult is null)
        {
            _jsModule ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import",
                $"./_content/{typeof(TextBox).Assembly.GetName().Name}/text-box/text-box.js");

            _jsAttachResult = await _jsModule.InvokeAsync<IJSObjectReference>("attach");
        }

        return _jsAttachResult;
    }

    private async Task RemoveJsAsync()
        => await DisposeJsAttachResultAsync();

    [LoggerMessage(Level = LogLevel.Error, Message = $"Disposing jsAttachResult failed")]
    private static partial void DisposingJsAttachResultFailed(ILogger logger, Exception ex);

    private async Task DisposeJsAttachResultAsync()
    {
        if (_jsAttachResult is not null)
        {
            try
            {
                await _jsAttachResult.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
            }
            catch (Exception ex)
            {
                DisposingJsAttachResultFailed(Logger, ex);
            }

            _jsAttachResult = null;
        }
    }

    /// <inheritdoc/>
    public async Task FocusAsync()
        => await _inputElementReference.FocusAsync();

    /// <summary>
    /// Select the content of the input element
    /// </summary>
    public async Task SelectContentAsync()
    {
        var jsAttachResult = await AttachJsAsync();

        if (jsAttachResult is null)
            return;

        await jsAttachResult.InvokeVoidAsync("selectContent", _inputElementReference);
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (_valuePassed != Value || UpdateKey != _updateKey)
        {
            _valuePassed = Value;
            _valueEntered = _valuePassed;
        }

        _shouldRender = true;
        _updateKey = UpdateKey;
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_selectContentAfterRender)
        {
            await SelectContentAsync();

            _selectContentAfterRender = false;
        }
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

    private async Task HandleValueChangingAsync(string? value)
    {
        if (DoesValuePassedDifferFrom(value))
        {
            if (ValueChanging.HasDelegate)
                await ValueChanging.InvokeAsync(value);
        }
    }

    private async Task HandleValueChangedAsync(string? value)
    {
        if (DoesValuePassedDifferFrom(value))
        {
            if (ValueChanged.HasDelegate)
                await ValueChanged.InvokeAsync(value);

            Value = value;
        }
    }

    private bool DoesValuePassedDifferFrom(string? value)
        => !string.Equals(value, _valuePassed, StringComparison.Ordinal);

    private async Task InputBlurAsync(FocusEventArgs _)
    {
        await HandleValueChangedAsync(_valueEntered);

        if (FocusLost.HasDelegate)
            await FocusLost.InvokeAsync(_valueEntered);
    }

    private async Task InputKeyUpAsync(KeyboardEventArgs args)
    {
        if (args.Code == "Escape")
        {
            if (!string.Equals(Value, _valueEntered, StringComparison.Ordinal))
            {
                _valueEntered = Value;
                _selectContentAfterRender = true;
                _shouldRender = true;

                await HandleValueChangedAsync(_valueEntered);

                if (EscapePressed.HasDelegate)
                    await EscapePressed.InvokeAsync(_valueEntered);
            }
        }
        else if (args.IsEnter())
        {
            await HandleValueChangedAsync(_valueEntered);

            if (EnterPressed.HasDelegate)
                await EnterPressed.InvokeAsync(_valueEntered);
        }
    }

    private async Task AfterOnInput()
        => await HandleValueChangingAsync(_valueEntered);
}
