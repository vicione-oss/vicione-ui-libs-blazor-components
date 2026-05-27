using System.Timers;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Resizing.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Services;

namespace ViciOne.Ui.Blazor.Components.Toolbar.Components;

/// <summary>
/// The primary component for a toolbar.
/// It handles the layout, visibility, and overflow logic for its children of type <see cref="IToolbarChild"/>.
/// </summary>
public sealed partial class Toolbar : ComponentBase, IAsyncDisposable, IToolbarItemParent
{
    private const double MenuContainerWidth = 24 + 32; // left margin + content

    private ElementReference _container;
    private ElementReference _menu;
    private double? _latestWidth;

    private int _itemsInMenuCount;

    private readonly List<IToolbarChild> _children = [];

    private bool _sizeChanged;

    // Limits the amount of resize calculations to the specified interval
    private readonly System.Timers.Timer _resizeRateLimitTimer = new()
    {
        AutoReset = true,
        Enabled = true,
        Interval = 25
    };

    [Inject] private IResizeObserver ResizeObserver { get; init; } = default!;

    /// <summary>
    /// Content to be rendered inside the toolbar.
    /// </summary>
    /// <remarks>
    /// Use component <see cref="ToolbarGroup"/>, <see cref="ToolbarButton"/> and / or
    /// <see cref="ToolbarContent"/> to render well-defined sections of content.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; init; }

    IReadOnlyList<IToolbarChild> IToolbarItemParent.Children => _children;

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _resizeRateLimitTimer.Elapsed += ResizeRateLimitTimerElapsed;

            ResizeObserver.ElementSizeChanged += OnElementSizeChanged;
            await ResizeObserver.ObserveAsync(_container);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        _resizeRateLimitTimer.Elapsed -= ResizeRateLimitTimerElapsed;
        _resizeRateLimitTimer.Stop();
        _resizeRateLimitTimer.Dispose();

        ResizeObserver.ElementSizeChanged -= OnElementSizeChanged;
        await ResizeObserver.UnobserveAsync(_container);
    }

    private void ResizeRateLimitTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // Sufficient to exchange value here (before handling) without an race condition guard.
        // Worst possible case is a parallel reset to true before we did the handling,
        // which will cause another loop with identical data once again after (= one useless handling call).
        if (Interlocked.CompareExchange(ref _sizeChanged, false, true))
            HandleSizeChanged();
    }

    void IToolbarItemParent.AddChild(IToolbarChild child)
    {
        if (_children.Any(x => x == child))
            return;

        _children.Add(child);
    }

    void IToolbarItemParent.RemoveChild(IToolbarChild child)
        => _children.Remove(child);

    private void OnElementSizeChanged(ElementSizeChangedEventArgs args)
    {
        if (args.ElementReference.Id != _container.Id)
            return;

        _latestWidth = args.DomRect.Width;

        _sizeChanged = true;
    }

    void IToolbarItemParent.ChildSizeChanged()
        => _sizeChanged = true;

    private void HandleSizeChanged()
    {
        if (_latestWidth is null)
            return;

        var availableWidth = _latestWidth.Value;
        if (GetMaxRequiredWidth(_children) > availableWidth)
            availableWidth -= MenuContainerWidth;

        var oldItemsInMenuCount = _itemsInMenuCount;
        _itemsInMenuCount = 0;

        HandleSizeChangedRecursive(_children, availableWidth);

        if (_itemsInMenuCount != oldItemsInMenuCount)
            InvokeAsync(StateHasChanged);
    }

    private double HandleSizeChangedRecursive(IEnumerable<IToolbarChild> children, double availableWidth)
    {
        var suppressAlignRight = false;

        foreach (var child in children)
        {
            if (child.DomRect is null)
                continue;

            var requiredWidth = GetRequiredWidth(child);
            var canFit = availableWidth >= requiredWidth;

            var group = child as ToolbarGroup;

            // once at least one item is hidden, hide all remaining
            var forceHide = _itemsInMenuCount > 0;

            if (canFit && !forceHide)
            {
                child.SetHidden(false);

                availableWidth -= requiredWidth;
            }
            else
            {
                child.SetHidden(true);

                if (group is null)
                    _itemsInMenuCount++;
            }

            availableWidth = HandleSizeChangedRecursive(child.Children, availableWidth);

            if (group is not null)
            {
                // group special: only render, if at least one child is rendered
                child.SetHidden(child.Children.All(c => c.IsHidden()));

                // group special 2: Suppress all right alignments except the very first
                if (group.AlignRight)
                {
                    if (suppressAlignRight)
                        group.SuppressAlignRight();

                    suppressAlignRight = true;
                }
            }

            child.Refresh();
        }

        return availableWidth;
    }

    private static double GetMaxRequiredWidth(IEnumerable<IToolbarChild> children)
    {
        var result = 0.0;
        foreach (var child in children)
        {
            result += GetRequiredWidth(child);
            result += GetMaxRequiredWidth(child.Children);
        }

        return result;
    }

    private static double GetRequiredWidth(IToolbarChild child)
    {
        if (child.DomRect is null)
            return 0;

        if (child is ToolbarItemBase { Visible: false })
            return 0;

        var style = child.Style;
        if (style is null)
            return 0;

        if (child is ToolbarGroup)
        {
            return style.MarginLeft +
                style.MarginRight +
                style.PaddingLeft +
                style.PaddingRight +
                style.BorderLeft +
                style.BorderRight;
        }

        return child.DomRect.Width + style.MarginLeft + style.MarginRight;
    }

    private async Task ShowMenuAsync()
        => await _menu.FocusAsync();
}
