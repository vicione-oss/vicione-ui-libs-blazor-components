using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.DropDown;
using ViciOne.Ui.Blazor.Components.Extensions;

namespace ViciOne.Ui.Blazor.Components.TagBox;

/// <summary>
/// Component for rendering a tag input element
/// </summary>
public sealed partial class TagBox : ComponentBase, IAsyncDisposable
{
    private ElementReference? _tagBox;
    private ElementReference? _inputElement;
    private DropDown<string>? _dropDown;
    private IJSObjectReference? _jsModule;
    private IJSObjectReference? _jsInstance;
    private bool _disposedAsync;
    private string _inputValue = string.Empty;
    private bool _alignInputElement;

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
    /// Specifies whether the TagBox hides selected items from the drop-down list
    /// </summary>
    [Parameter]
    public bool HideSelectedItems { get; set; }

    /// <summary>
    /// Specifies whether users are allowed to input custom tags
    /// </summary>
    [Parameter]
    public bool AllowCustomTags { get; set; }

    /// <summary>
    /// Collection of tags of specific element
    /// </summary>
    [Parameter]
    public IEnumerable<string> Tags { get; set; } = [];

    /// <summary>
    /// Collection of available tags
    /// </summary>
    [Parameter]
    public IEnumerable<string> AvailableTags { get; set; } = [];

    /// <summary>
    /// Raised when the <see cref="Tags" /> has changed
    /// </summary>
    [Parameter]
    public EventCallback<IEnumerable<string>> TagsChanged { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private ILogger<TagBox> Logger { get; set; } = default!;

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import",
                $"./_content/{typeof(TagBox).Assembly.GetName().Name}/tag-box/tag-box.js");

            _jsInstance = await _jsModule.InvokeConstructorAsync("TagBox", Logger, _tagBox, _inputElement);

            // The compiled TypeScript runs after the first render, so the width and height values are not set yet. Re-rendering here applies
            // those values and prevents jumping effect of the TagBox.
            StateHasChanged();
        }
        else if (_jsInstance is not null && _alignInputElement)
        {
            await _jsInstance.InvokeVoidAsync("alignInputElement");

            _alignInputElement = false;
        }
    }

    private async Task InputKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.IsEnter())
        {
            if (!string.IsNullOrWhiteSpace(_inputValue))
                await AddTagAsync(_inputValue.Trim());
        }
        else if (e.Key is "Escape")
        {
            _inputValue = string.Empty;
            _alignInputElement = true;
        }
    }

    private async Task InputFocusOutAsync(FocusEventArgs _)
    {
        if (!string.IsNullOrWhiteSpace(_inputValue))
            await AddTagAsync(_inputValue.Trim());

        if (_dropDown is not null)
            await _dropDown.HideAsync();
    }

    private async Task InputFocusAsync(FocusEventArgs _)
    {
        if (_dropDown is not null && !ReadOnly)
            await _dropDown.ShowAsync();
    }

    private async Task AddTagAsync(string tag)
    {
        _inputValue = string.Empty;

        _alignInputElement = true;

        var isNewTag = !Tags.Contains(tag);

        if ((AllowCustomTags && isNewTag) || (!AllowCustomTags && AvailableTags.Contains(tag) && isNewTag))
        {
            Tags = Tags.Append(tag);

            await TagsChanged.InvokeAsync(Tags);
        }
    }

    private async Task RemoveTagAsync(string tag)
    {
        _alignInputElement = true;

        Tags = Tags.Except([tag]);

        await TagsChanged.InvokeAsync(Tags);
    }

    private async Task NotifyTagsChangedAsync()
        => await TagsChanged.InvokeAsync(Tags);

    private IEnumerable<string> FilterAvailableTags()
    {
        var filtered = AvailableTags.AsEnumerable();

        if (HideSelectedItems)
            filtered = filtered.Where(t => !Tags.Contains(t));

        if (!string.IsNullOrWhiteSpace(_inputValue))
            filtered = filtered.Where(t => t.Contains(_inputValue.Trim(), StringComparison.OrdinalIgnoreCase));

        return filtered;
    }

    private async Task DisposeJsInstanceAsync()
    {
        if (_jsInstance is not null)
        {
            try
            {
                await _jsInstance.InvokeVoidAsync("dispose");
            }
            catch (JSDisconnectedException)
            {
                // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
            }
            catch (Exception ex)
            {
                InvokingJsInstanceDisposeFailed(Logger, ex);
            }

            try
            {
                await _jsInstance.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
            }
            catch (Exception ex)
            {
                DisposingJsInstanceFailed(Logger, ex);
            }

            _jsInstance = null;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await DisposeJsInstanceAsync();

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

        _disposedAsync = true;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = $"Invoking jsInstance.dispose() failed")]
    private static partial void InvokingJsInstanceDisposeFailed(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = $"Disposing jsInstance failed")]
    private static partial void DisposingJsInstanceFailed(ILogger logger, Exception ex);
}
