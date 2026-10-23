using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Accordion.Models;
using ViciOne.Ui.Blazor.Components.Extensions;

namespace ViciOne.Ui.Blazor.Components.Accordion.Components;

/// <inheritdoc cref="IAccordionItem"/>
public sealed partial class AccordionItem : ComponentBase, IAccordionItem, IAsyncDisposable
{
    private ElementReference _rootElementReference;
    private IJSObjectReference? _jsModule;
    private Task<IJSObjectReference?>? _importJsModuleTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _disposedAsync;

    /// <summary>
    /// Gets or sets the display text rendered in the item header.
    /// </summary>
    [Parameter, EditorRequired]
    public required string Text { get; set; }

    /// <summary>
    /// Gets or sets the HTML <c>title</c> attribute applied to the root element.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets whether the item is currently expanded and its content is visible.
    /// </summary>
    [Parameter]
    public bool Expanded { get; set; }

    /// <summary>
    /// Raised when <see cref="Expanded"/> has changed.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ExpandedChanged { get; set; }

    /// <summary>
    /// Gets or sets an additional CSS class applied to the root element.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    /// <remarks>The icon size should be 24 x 24 pixels.</remarks>
    [Parameter]
    public string? IconCssClass { get; set; }

    /// <inheritdoc/>
    /// <remarks>The icon size should be 24 x 24 pixels.</remarks>
    [Parameter]
    public Uri? IconUrl { get; set; }

    /// <inheritdoc/>
    /// <remarks>The icon size should be 24 x 24 pixels.</remarks>
    [Parameter]
    public string? IconData { get; set; }

    /// <summary>
    /// Gets or sets the content rendered inside the item when it is expanded.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// A callback invoked before this item is expanded.
    /// </summary>
    [Parameter]
    public EventCallback<AccordionItemCancelEventArgs> OnBeforeExpand { get; set; }

    /// <summary>
    /// A callback invoked before this item is collapsed.
    /// </summary>
    [Parameter]
    public EventCallback<AccordionItemCancelEventArgs> OnBeforeCollapse { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private ILogger<AccordionItem> Logger { get; set; } = default!;

    internal async Task ExpandAsync()
    {
        var args = new AccordionItemCancelEventArgs { Sender = this };

        if (OnBeforeExpand.HasDelegate)
            await OnBeforeExpand.InvokeAsync(args);

        if (!args.Cancel)
        {
            Expanded = true;

            if (ExpandedChanged.HasDelegate)
                await ExpandedChanged.InvokeAsync(Expanded);
        }
    }

    internal async Task CollapseAsync()
    {
        var args = new AccordionItemCancelEventArgs { Sender = this };

        if (OnBeforeCollapse.HasDelegate)
            await OnBeforeCollapse.InvokeAsync(args);

        if (!args.Cancel)
        {
            Expanded = false;

            if (ExpandedChanged.HasDelegate)
                await ExpandedChanged.InvokeAsync(Expanded);
        }
    }

    private async Task OnHeaderClickAsync()
    {
        if (Expanded)
            await CollapseAsync();
        else
            await ExpandAsync();
    }

    private Task<IJSObjectReference?> ImportJsModuleAsync()
        => _importJsModuleTask ??= CreateJsModuleAsync();

    private async Task<IJSObjectReference?> CreateJsModuleAsync()
    {
        if (_disposedAsync)
            return null;

        _jsModule = await JsRuntime.ImportModuleAsync(
            $"./_content/{typeof(AccordionItem).Assembly.GetName().Name}/accordion/components/accordion-item.js", Logger);

        // An item removed while its script was loading has already run its disposal without seeing the module.
        if (_disposedAsync)
        {
            await _jsModule.DisposeAsync(Logger);
            _jsModule = null;
        }

        return _jsModule;
    }

    /// <summary>
    /// Scrolls the item the shortest distance that brings it fully into view; an item already in view does not move.
    /// </summary>
    /// <remarks>Does nothing before the item has rendered.</remarks>
    public async Task ScrollIntoViewAsync()
    {
        if (_disposedAsync || _rootElementReference.Id is null)
            return;

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            var jsModule = await ImportJsModuleAsync();

            if (_disposedAsync)
                return;

            await jsModule.InvokeVoidAsync("scrollIntoView", Logger, cancellationToken, _rootElementReference);
        }
        catch (ObjectDisposedException)
        {
            // CancellationTokenSource already disposed, nothing we can do, return gracefully
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();

        await _jsModule.DisposeAsync(Logger);
        _jsModule = null;
    }
}
