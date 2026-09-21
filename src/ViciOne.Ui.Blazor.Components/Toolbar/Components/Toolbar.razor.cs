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
    private readonly List<IToolbarChild> _menuChildren = [];

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
    /// <para>
    /// When at least one item does not fit into the toolbar, the content is rendered a second time inside the menu,
    /// so every component inside it exists twice. Inside a <see cref="ToolbarContent"/>, use
    /// <see cref="IToolbarContent.IsInMenu"/> on the context of <see cref="ToolbarContent.ChildContent"/> to tell them apart.
    /// </para>
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; init; }

    IReadOnlyList<IToolbarChild> IToolbarItemParent.Children => _children;

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _resizeRateLimitTimer.Elapsed += ResizeRateLimitTimerElapsedAsync;

            ResizeObserver.ElementSizeChanged += OnElementSizeChanged;
            await ResizeObserver.ObserveAsync(_container);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        _resizeRateLimitTimer.Elapsed -= ResizeRateLimitTimerElapsedAsync;
        _resizeRateLimitTimer.Stop();
        _resizeRateLimitTimer.Dispose();

        ResizeObserver.ElementSizeChanged -= OnElementSizeChanged;
        await ResizeObserver.UnobserveAsync(_container);
    }

    private async void ResizeRateLimitTimerElapsedAsync(object? sender, ElapsedEventArgs e)
    {
        // Sufficient to exchange value here (before handling) without an race condition guard.
        // Worst possible case is a parallel reset to true before we did the handling,
        // which will cause another loop with identical data once again after (= one useless handling call).
        if (!Interlocked.CompareExchange(ref _sizeChanged, false, true))
            return;

        try
        {
            // Timer callbacks run on a thread pool thread, but component re-render and state changes need to be
            // synchronized with the renderer's synchronization context, using InvokeAsync().
            await InvokeAsync(HandleSizeChanged);
        }
        catch (Exception exception)
        {
            await DispatchExceptionAsync(exception);
        }
    }

    void IToolbarItemParent.AddChild(IToolbarChild child)
    {
        var children = child.IsInMenu() ? _menuChildren : _children;

        if (children.Any(x => x == child))
            return;

        children.Add(child);

        if (child.IsInMenu())
            SynchronizeMenuChildren();
    }

    void IToolbarItemParent.RemoveChild(IToolbarChild child)
    {
        if (!child.IsInMenu())
        {
            _children.Remove(child);
            return;
        }

        _menuChildren.Remove(child);

        SynchronizeMenuChildren();
    }

    void IToolbarItemParent.MenuChildrenChanged()
        => SynchronizeMenuChildren();

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

        SynchronizeMenuChildren();

        if (_itemsInMenuCount != oldItemsInMenuCount)
            StateHasChanged();
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

    private void SynchronizeMenuChildren()
    {
        List<IToolbarChild> changedMenuChildren = [];

        SynchronizeMenuChildren(_children, _menuChildren, changedMenuChildren);

        foreach (var menuChild in changedMenuChildren)
            menuChild.Refresh();
    }

    private static void SynchronizeMenuChildren(
        IReadOnlyList<IToolbarChild> children,
        IReadOnlyList<IToolbarChild> menuChildren,
        List<IToolbarChild> changedMenuChildren)
    {
        foreach (var (child, menuChild) in children.Zip(menuChildren))
            SynchronizeMenuChild(child, menuChild, changedMenuChildren);

        foreach (var menuChild in menuChildren.Skip(children.Count))
            SynchronizeMenuChild(null, menuChild, changedMenuChildren);
    }

    private static void SynchronizeMenuChild(IToolbarChild? child, IToolbarChild menuChild, List<IToolbarChild> changedMenuChildren)
    {
        var counterpart = child?.GetType() == menuChild.GetType() ? child : null;

        SynchronizeMenuChildren(counterpart?.Children ?? [], menuChild.Children, changedMenuChildren);

        var hidden = IsHiddenInMenu(counterpart, menuChild);
        if (menuChild.IsHidden() == hidden)
            return;

        menuChild.SetHidden(hidden);
        changedMenuChildren.Add(menuChild);
    }

    private static bool IsHiddenInMenu(IToolbarChild? counterpart, IToolbarChild menuChild)
    {
        if (menuChild is ToolbarGroup)
            return menuChild.Children.All(c => c.IsHidden());

        if (counterpart is null)
            return true;

        return !counterpart.IsHidden();
    }

    private async Task ShowMenuAsync()
        => await _menu.FocusAsync();
}
