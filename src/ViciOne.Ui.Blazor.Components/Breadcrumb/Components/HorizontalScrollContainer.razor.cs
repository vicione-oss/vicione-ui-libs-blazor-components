using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Resizing.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Services;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace ViciOne.Ui.Blazor.Components.Breadcrumb.Components;

/// <summary>
/// A container component that provides horizontal scrolling logic for its content.
/// It monitors its own dimensions and content size to manage overflow.
/// It supports scrolling in steps where the step size is handled by the caller to support non-equidistant steps.
/// </summary>
public sealed partial class HorizontalScrollContainer : ComponentBase, IAsyncDisposable
{
    private static readonly MonochromeIconSize s_iconSize = MonochromeIconSize.SmallMedium;

    private readonly string _leftIcon =
        MonochromeIconName.ExpanderLightLeft.GetCssClasses(s_iconSize).ToSpaceSeparated();

    private readonly string _rightIcon =
        MonochromeIconName.ExpanderLightRight.GetCssClasses(s_iconSize).ToSpaceSeparated();

    private ElementReference _contentContainer;
    private ElementReference _content;

    private double? _contentContainerWidth;
    private double? _contentWidth;

    private double? _lastShownPixel;

    private double? _offset;
    private int _scrollStep;

    private bool _hasScroll;

    private double MaxOffset
    {
        get
        {
            if (_contentWidth == null || _contentContainerWidth == null)
                return 0;

            return _contentWidth.Value - _contentContainerWidth.Value;
        }
    }

    /// <summary>
    /// Gets or sets the last shown pixel of the content where 0 is the very first content pixel.
    /// </summary>
    [Parameter, EditorRequired]
    public required double? LastShownPixel { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of steps available for scrolling.
    /// </summary>
    [Parameter, EditorRequired]
    public required int MaxScrollStepCount { get; set; }

    /// <summary>
    /// An event callback that triggers when a scroll event occurs, providing the new scroll step.
    /// </summary>
    [Parameter, EditorRequired]
    public required EventCallback<int> OnScroll { get; set; }

    /// <summary>
    /// Gets or sets the UI content to be rendered inside the scrollable container.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Inject] private IResizeObserver ResizeObserver { get; init; } = default!;

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (!_lastShownPixel.NearlyEquals(LastShownPixel))
        {
            _lastShownPixel = LastShownPixel;
            SetScrollPosition(_lastShownPixel);
        }
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            ResizeObserver.ElementSizeChangedAsync += OnElementSizeChangedAsync;
            await ResizeObserver.ObserveAsync(_contentContainer);
            await ResizeObserver.ObserveAsync(_content);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await ResizeObserver.UnobserveAsync(_content);
        await ResizeObserver.UnobserveAsync(_contentContainer);

        ResizeObserver.ElementSizeChangedAsync -= OnElementSizeChangedAsync;
    }

    private async Task OnElementSizeChangedAsync(ElementSizeChangedEventArgs args)
    {
        var width = args.DomRect.Width;

        var changedValue = false;
        if (args.ElementReference.Id == _contentContainer.Id && !width.NearlyEquals(_contentContainerWidth))
        {
            _contentContainerWidth = width;
            changedValue = true;
        }
        else if (args.ElementReference.Id == _content.Id && !width.NearlyEquals(_contentWidth))
        {
            _contentWidth = width;
            changedValue = true;
        }

        if (!changedValue)
            return;

        var requiresScroll = RequiresScroll();
        if (requiresScroll && _hasScroll)
        {
            if (_scrollStep == MaxScrollStepCount - 1)
                SetScrollPosition(_lastShownPixel);
            else
                await ScrollToLastAsync();
        }
        else if (requiresScroll != _hasScroll)
        {
            _hasScroll = requiresScroll;
            if (!requiresScroll)
                _offset = 0;

            // sufficient to  rerender here, since adding the scrolling triggers a resize which then handles it
            await InvokeAsync(StateHasChanged);
        }
    }

    private bool RequiresScroll()
        => _contentWidth != null && _contentContainerWidth != null &&
           _contentWidth > _contentContainerWidth;

    private async Task ScrollToLastAsync()
    {
        _scrollStep = MaxScrollStepCount - 1;

        await OnScroll.InvokeAsync(_scrollStep);
    }

    private void SetScrollPosition(double? lastShownPixel)
    {
        if (_contentContainerWidth == null || lastShownPixel == null)
            return;

        var offset = lastShownPixel.Value - _contentContainerWidth.Value;

        var maxScrollingPosition = MaxOffset > 0 ? MaxOffset : 0;
        _offset = Math.Clamp(offset, 0, maxScrollingPosition);

        InvokeAsync(StateHasChanged);
    }

    private bool CanScrollLeft()
        => _offset > 0 && _scrollStep > 0;

    private bool CanScrollRight()
        => _offset < MaxOffset && _scrollStep < MaxScrollStepCount - 1;

    private async Task ScrollLeftAsync()
        => await OnScroll.InvokeAsync(--_scrollStep);

    private async Task ScrollRightAsync()
        => await OnScroll.InvokeAsync(++_scrollStep);
}
