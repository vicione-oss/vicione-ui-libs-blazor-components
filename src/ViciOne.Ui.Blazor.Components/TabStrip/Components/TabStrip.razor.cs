using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.TabStrip.Enums;
using ViciOne.Ui.Blazor.Components.TabStrip.Models;

namespace ViciOne.Ui.Blazor.Components.TabStrip.Components;

/// <summary>
/// A component that renders a collection of <see cref="Tab"/> instances with horizontal scroll support.
/// </summary>
public sealed partial class TabStrip : ComponentBase, ITabStrip, IAsyncDisposable
{
    private readonly List<TabContext> _tabContexts = [];

    private ElementReference _scrollContainer;

    private IJSObjectReference? _jsModule;
    private IJSObjectReference? _jsAttachResult;
    private DotNetObjectReference<TabStrip>? _dotNetObjectReference;
    private bool _disposedAsync;

    private bool _initialized;
    private bool _canScrollLeft;
    private bool _canScrollRight;

    private TabSize? _renderedTabSize;
    private bool _scrollToActiveTabAfterRender;

    /// <inheritdoc/>
    [Parameter]
    public int ActiveTabIndex { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public EventCallback<int> ActiveTabIndexChanged { get; set; }

    /// <summary>
    /// Size of the tabs
    /// </summary>
    [Parameter]
    public TabSize TabSize { get; set; } = TabSize.Small;

    /// <summary>
    /// Gets or sets the content containing <see cref="Tab"/> components.
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private ILogger<TabStrip> Logger { get; set; } = default!;

    void ITabStrip.AddTab(ITab tab)
    {
        if (!_tabContexts.Exists(c => c.Instance == tab))
            _tabContexts.Add(new TabContext { Instance = tab });
    }

    void ITabStrip.RemoveTab(ITab tab)
        => _tabContexts.RemoveAll(c => c.Instance == tab);

    int ITabStrip.GetTabIndex(ITab tab)
        => _tabContexts.FindIndex(c => c.Instance == tab);

    void ITabStrip.RegisterTabElement(ITab tab, ElementReference elementReference)
    {
        var context = _tabContexts.Find(c => c.Instance == tab);

        context?.ElementReference = elementReference;
    }

    async Task ITabStrip.ChangeActiveTabIndexAsync(int previousIndex, int newIndex)
    {
        if (previousIndex == newIndex)
            return;

        if (!IsValidTabIndex(previousIndex) || !IsValidTabIndex(newIndex))
            return;

        ActiveTabIndex = newIndex;

        if (ActiveTabIndexChanged.HasDelegate)
            await ActiveTabIndexChanged.InvokeAsync(ActiveTabIndex);

        var renderTasks = new List<Task>
        {
            _tabContexts[previousIndex].Instance.RenderAsync(),
            _tabContexts[newIndex].Instance.RenderAsync()
        };

        // When the parent changed ActiveTabIndex via binding (e.g. to a third tab) during the callback,
        // that tab also needs to re-render so its visual state stays in sync.
        if (ActiveTabIndex != newIndex && ActiveTabIndex != previousIndex && IsValidTabIndex(ActiveTabIndex))
            renderTasks.Add(_tabContexts[ActiveTabIndex].Instance.RenderAsync());

        await Task.WhenAll(renderTasks);

        await ScrollToActiveTabAsync();
    }

    /// <summary>
    /// Called by the TypeScript counterpart whenever the scroll position or content size of the tab strip changes.
    /// </summary>
    [JSInvokable]
    public async Task ScrollStateChangedAsync(bool canScrollLeft, bool canScrollRight)
    {
        if (canScrollLeft == _canScrollLeft &&
            canScrollRight == _canScrollRight &&
            _initialized)
        {
            return;
        }

        _canScrollLeft = canScrollLeft;
        _canScrollRight = canScrollRight;
        _initialized = true;

        await InvokeAsync(StateHasChanged);
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (_renderedTabSize.HasValue && _renderedTabSize.Value != TabSize)
            _scrollToActiveTabAfterRender = true;
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await AttachJsAsync();

        _renderedTabSize = TabSize;

        if (_scrollToActiveTabAfterRender)
        {
            _scrollToActiveTabAfterRender = false;
            await ScrollToActiveTabAsync(scrollBehavior: "auto");
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

        _dotNetObjectReference?.Dispose();
        _dotNetObjectReference = null;
    }

    private async Task ScrollLeftAsync()
    {
        if (!_canScrollLeft || _jsAttachResult is null)
            return;

        await _jsAttachResult.InvokeVoidAsync("scrollLeft");
    }

    private async Task ScrollRightAsync()
    {
        if (!_canScrollRight || _jsAttachResult is null)
            return;

        await _jsAttachResult.InvokeVoidAsync("scrollRight");
    }

    private async Task ScrollToActiveTabAsync(string scrollBehavior = "smooth")
    {
        if (!IsValidTabIndex(ActiveTabIndex) || _jsAttachResult is null)
            return;

        var tabElement = _tabContexts[ActiveTabIndex].ElementReference;
        if (tabElement is null)
            return;

        await _jsAttachResult.InvokeVoidAsync("scrollToActiveTab", tabElement, scrollBehavior);
    }

    private bool IsValidTabIndex(int index)
        => index >= 0 && index < _tabContexts.Count;

    private async Task AttachJsAsync()
    {
        if (_disposedAsync)
            return;

        if (_jsAttachResult is null)
        {
            _jsModule ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import",
                $"./_content/{typeof(TabStrip).Assembly.GetName().Name}/tab-strip/components/tab-strip.js");

            _dotNetObjectReference ??= DotNetObjectReference.Create(this);

            _jsAttachResult = await _jsModule.InvokeAsync<IJSObjectReference>("attach",
                _dotNetObjectReference, _scrollContainer);
        }
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

    [LoggerMessage(Level = LogLevel.Error, Message = $"Invoking jsAttachResult.dispose() failed")]
    private static partial void InvokingJsAttachResultDisposeFailed(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = $"Disposing jsAttachResult failed")]
    private static partial void DisposingJsAttachResultFailed(ILogger logger, Exception ex);
}
