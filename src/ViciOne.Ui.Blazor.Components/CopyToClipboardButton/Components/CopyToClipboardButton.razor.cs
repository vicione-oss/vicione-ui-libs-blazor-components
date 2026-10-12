using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.CopyToClipboardButton.Enums;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Helpers;
using ViciOne.Ui.Blazor.Components.Tooltip.Enums;

namespace ViciOne.Ui.Blazor.Components.CopyToClipboardButton.Components;

/// <summary>
/// Component for rendering a button that copies text to the clipboard and confirms it with a tooltip
/// </summary>
public sealed partial class CopyToClipboardButton : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _jsModule;
    private IJSObjectReference? _jsInstance;
    private Task<IJSObjectReference?>? _attachJsTask;

    private ElementReference _element;

    private bool _disposed;

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#attr-title">title</see> and
    /// <see href="https://w3c.github.io/aria/#aria-label">aria-label</see> attribute
    /// </summary>
    /// <remarks>
    /// Defaults to the localized verb "Copy".
    /// </remarks>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// True when user interaction should be allowed, otherwise false
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Provides the text to copy, called on every click
    /// </summary>
    [Parameter, EditorRequired]
    public required Func<Task<string>> TextToCopy { get; set; }

    /// <summary>
    /// Size of the button
    /// </summary>
    [Parameter]
    public CopyToClipboardButtonSize Size { get; set; } = CopyToClipboardButtonSize.Medium;

    /// <summary>
    /// Placement of the tooltip confirming that the text has been copied
    /// </summary>
    [Parameter]
    public TooltipPlacement TooltipPlacement { get; set; } = TooltipPlacement.Top;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private ILogger<CopyToClipboardButton> Logger { get; set; } = default!;

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await RippleModuleLoader.LoadAsync(JsRuntime, Logger);
        await AttachJsAsync();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposed, true, false))
            return;

        await DisposeJsAsync();
    }

    private async Task ButtonClickAsync()
    {
        var text = await TextToCopy();

        if (_disposed)
            return;

        var jsInstance = await AttachJsAsync();

        await jsInstance.InvokeVoidAsync("copy", Logger, CancellationToken.None, text);
    }

    private Task<IJSObjectReference?> AttachJsAsync()
        => _attachJsTask ??= CreateJsInstanceAsync();

    private async Task<IJSObjectReference?> CreateJsInstanceAsync()
    {
        if (_disposed)
            return null;

        var assemblyName = typeof(CopyToClipboardButton).Assembly.GetName().Name;

        _jsModule = await JsRuntime.ImportModuleAsync(
            $"./_content/{assemblyName}/copy-to-clipboard-button/components/copy-to-clipboard-button.js", Logger);

        _jsInstance = await _jsModule.InvokeConstructorAsync("CopyToClipboardButton", Logger,
            _element, Localization.CopyToClipboardButton.Copied);

        // A disposal during the import or construction found nothing to dispose.
        if (_disposed)
            await DisposeJsAsync();

        return _jsInstance;
    }

    private async Task DisposeJsAsync()
    {
        await _jsInstance.InvokeVoidAsync("dispose", Logger);
        await _jsInstance.DisposeAsync(Logger);
        _jsInstance = null;

        await _jsModule.DisposeAsync(Logger);
        _jsModule = null;
    }
}
