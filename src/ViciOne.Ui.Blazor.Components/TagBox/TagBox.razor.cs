using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;

namespace ViciOne.Ui.Blazor.Components.TagBox;

/// <summary>
/// Component for rendering a tag input element
/// </summary>
public sealed partial class TagBox : ComponentBase, IAsyncDisposable
{
    private ElementReference? _tagBox;
    private ElementReference? _inputElement;
    private IJSObjectReference? _jsModule;
    private IJSObjectReference? _jsAttachResult;
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

            _jsAttachResult = await _jsModule.InvokeAsync<IJSObjectReference>("attach", _tagBox, _inputElement);
        }
        else if (_jsAttachResult is not null && _alignInputElement)
        {
            await _jsAttachResult.InvokeVoidAsync("alignInputElement");

            _alignInputElement = false;
        }
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.IsEnter())
        {
            var highlightedIndex = await GetHighlightedIndexAsync();
            var filteredTags = FilterAvailableTags().ToList();

            if (highlightedIndex >= 0 && highlightedIndex < filteredTags.Count)
            {
                await AddOrRemoveTagAsync(filteredTags[highlightedIndex]);
                await ResetHighlightAsync();
            }
            else if (!string.IsNullOrWhiteSpace(_inputValue))
            {
                await AddTagAsync(_inputValue.Trim());
            }
        }
        else if (e.Key is "Escape")
        {
            _inputValue = string.Empty;
            _alignInputElement = true;
            await ResetHighlightAsync();
        }
    }

    private async Task<int> GetHighlightedIndexAsync()
    {
        if (_jsAttachResult is not null)
            return await _jsAttachResult.InvokeAsync<int>("getHighlightedIndex");

        return -1;
    }

    private async Task ResetHighlightAsync()
    {
        if (_jsAttachResult is not null)
            await _jsAttachResult.InvokeVoidAsync("resetHighlight");
    }

    private async Task HandleFocusOutAsync(FocusEventArgs _)
    {
        if (!string.IsNullOrWhiteSpace(_inputValue))
            await AddTagAsync(_inputValue.Trim());
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

    private async Task AddOrRemoveTagAsync(string tag)
    {
        if (Tags.Contains(tag))
            await RemoveTagAsync(tag);
        else
            await AddTagAsync(tag);
    }

    private IEnumerable<string> FilterAvailableTags()
    {
        var filtered = AvailableTags.AsEnumerable();

        if (HideSelectedItems)
            filtered = filtered.Where(t => !Tags.Contains(t));

        if (!string.IsNullOrEmpty(_inputValue))
            filtered = filtered.Where(t => t.Contains(_inputValue, StringComparison.OrdinalIgnoreCase));

        return filtered;
    }

    private async Task DisposeJsAttachResultAsync()
    {
        if (_jsAttachResult is not null)
        {
            try
            {
                await _jsAttachResult.InvokeVoidAsync("dispose");
            }
            catch (JSDisconnectedException)
            {
                // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
            }
            catch (Exception ex)
            {
                InvokingJsAttachResultDisposeFailed(Logger, ex);
            }

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
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await DisposeJsAttachResultAsync();

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

    [LoggerMessage(Level = LogLevel.Error, Message = $"Invoking jsAttachResult.dispose() failed")]
    private static partial void InvokingJsAttachResultDisposeFailed(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = $"Disposing jsAttachResult failed")]
    private static partial void DisposingJsAttachResultFailed(ILogger logger, Exception ex);
}
