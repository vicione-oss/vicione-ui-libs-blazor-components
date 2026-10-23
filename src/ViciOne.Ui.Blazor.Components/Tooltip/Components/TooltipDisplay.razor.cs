using System.Drawing;
using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Tooltip.Models;
using ViciOne.Ui.Blazor.Components.Tooltip.Services;
using ViciOne.Ui.Blazor.Components.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tooltip.Components;

/// <summary>
/// Displays the tooltips registered in <see cref="TooltipService"/>.
/// </summary>
public sealed partial class TooltipDisplay : ComponentBase, IAsyncDisposable
{
    private const int TooltipPositioningMarginPx = 10;

    private bool _disposed;
    private IJSObjectReference? _jsModule;
    private readonly System.Timers.Timer _renderDelayTimer = new()
    {
        AutoReset = false,
        Enabled = false,
        Interval = 50,
    };
    private ICollection<TooltipInfo> _renderingTooltips = [];
    private bool _shouldRender;
    private bool _shouldRenderDelayElapsed;
    private TooltipService? _tooltipService;
    private Size _windowSize = Size.Empty;

    [CascadingParameter] private TooltipService? CascadedTooltipService { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private ILogger<TooltipDisplay> Logger { get; set; } = default!;
    [Inject] private IServiceProvider ServiceProvider { get; set; } = default!;

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposed, true, false))
            return;

        if (_tooltipService is not null)
        {
            _tooltipService.PreRenderingRequested -= OnPreRenderingRequestedAsync;
            _tooltipService.TooltipsChanged -= OnTooltipsChangedAsync;
        }

        _renderDelayTimer.Elapsed -= OnRenderDelayTimerElapsedAsync;
        _renderDelayTimer.Dispose();

        await _jsModule.DisposeAsync(Logger);
        _jsModule = null;
    }

    private Point GetTooltipRootPosition(TooltipInfo info, bool useThirds)
    {
        bool moveLeft, moveUp;
        if (useThirds)
        {
            var windowThirds = new Size((int)Math.Round(_windowSize.Width / 3.0), (int)Math.Round(_windowSize.Height / 3.0));
            moveLeft = info.Position.X > windowThirds.Width * 2;
            moveUp = info.Position.Y > windowThirds.Height * 2;
        }
        else
        {
            moveLeft = info.Position.X + info.Size.Width > _windowSize.Width;
            moveUp = info.Position.Y + info.Size.Height > _windowSize.Height;
        }

        return new Point(
            moveLeft
                ? info.Position.X - info.Size.Width - TooltipPositioningMarginPx
                : info.Position.X + TooltipPositioningMarginPx,
            moveUp
                ? info.Position.Y - info.Size.Height - TooltipPositioningMarginPx
                : info.Position.Y + TooltipPositioningMarginPx
        );
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import",
                $"./_content/{typeof(TooltipDisplay).Assembly.GetName().Name}/tooltip/components/tooltip-display.js");

            // Removed while its script was loading: the disposal ran without seeing the module.
            if (_disposed)
            {
                await _jsModule.DisposeAsync(Logger);
                _jsModule = null;
                return;
            }
        }

        if (_renderingTooltips.Count == 0 || _jsModule is null)
            return;

        _windowSize = await _jsModule.InvokeAsync<Size>("getWindowSize");

        foreach (var tooltip in _renderingTooltips.Where(dt => !dt.PreRendered))
        {
            tooltip.Size = await _jsModule.InvokeAsync<Size>("getTooltipSize", tooltip.Id);
            tooltip.PreRendered = true;
        }
    }

    private async void OnTooltipsChangedAsync()
    {
        if (_disposed || _tooltipService is null)
            return;

        _renderingTooltips = _tooltipService.GetTooltipInfos();

        _shouldRender = true;
        await InvokeAsync(StateHasChanged);
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        var injectedService = ServiceProvider.GetService(typeof(TooltipService));
        if (injectedService is not null)
        {
            _tooltipService = (TooltipService)injectedService;
        }
        else if (CascadedTooltipService is not null)
        {
            _tooltipService = CascadedTooltipService;
        }
        else
        {
            throw new ArgumentNullException(nameof(_tooltipService),
                $"Either a cascading value or a dependency injected service of type {nameof(TooltipService)} have to be available.");
        }

        _tooltipService.PreRenderingRequested += OnPreRenderingRequestedAsync;
        _tooltipService.TooltipsChanged += OnTooltipsChangedAsync;

        _renderDelayTimer.Elapsed += OnRenderDelayTimerElapsedAsync;
    }

    private async void OnPreRenderingRequestedAsync()
    {
        if (_disposed || _tooltipService is null)
            return;

        _renderingTooltips = _tooltipService.GetTooltipInfos();

        _shouldRender = true;
        await InvokeAsync(StateHasChanged);
    }

    private async void OnRenderDelayTimerElapsedAsync(object? sender, ElapsedEventArgs e)
    {
        if (_disposed)
            return;

        _shouldRender = true;
        _shouldRenderDelayElapsed = true;

        await InvokeAsync(StateHasChanged);
    }

    /// <inheritdoc/>
    protected override bool ShouldRender()
    {
        // A handler already running when the display was removed must not restart the disposed timer.
        if (_disposed)
            return false;

        if (_shouldRender)
        {
            _shouldRender = false;

            if (_shouldRenderDelayElapsed)
            {
                _shouldRenderDelayElapsed = false;
                return true;
            }

            _renderDelayTimer.Stop();
            _renderDelayTimer.Start();
            return true;
        }

        return false;
    }
}
