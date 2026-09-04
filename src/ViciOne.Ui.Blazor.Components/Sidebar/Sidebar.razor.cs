using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.Resizeable.Components;
using ViciOne.Ui.Blazor.Components.Resizeable.Services;
using ViciOne.Ui.Blazor.Components.Sidebar.Enums;

namespace ViciOne.Ui.Blazor.Components.Sidebar;

/// <summary>
/// A component for rendering a sidebar with support for a resize handle to allow adjusting the width of the sidebar via drag.
/// The resize handle is rendered when <see cref="Mode"/> is set to <see cref="SidebarMode.Fluid"/>.
/// </summary>
/// <remarks>
/// https://en.wikipedia.org/wiki/Sidebar_(computing)
/// </remarks>
public sealed partial class Sidebar : ComponentBase, IResizeable, IResizeContainer, IAsyncDisposable
{
    private readonly List<IResizeHandle> _resizeHandles = [];

    private ElementReference _sidebarElementReference;
    private ElementReference _resizeContainerElementReference;

    private Task? _resizeInteractionAttachTask;
    private bool _reattachResizeInteraction;
    private bool _disposedAsync;

    private int _width;
    private int _fluidMinimumWidth;
    private int _fluidMaximumWidth;
    private SidebarPlacement _placement;

    /// <summary>
    /// Placement in the outer container
    /// </summary>
    [Parameter]
    public SidebarPlacement Placement { get; set; }

    /// <inheritdoc cref="SidebarMode"/>
    [Parameter]
    public SidebarMode Mode { get; set; }

    /// <summary>
    /// Width of the sidebar when <see cref="Mode"/> is <see cref="SidebarMode.Compact"/>
    /// </summary>
    [Parameter, EditorRequired]
    public required int CompactWidth { get; set; }

    /// <summary>
    /// Minimum width of the sidebar when <see cref="Mode"/> is <see cref="SidebarMode.Fluid"/>
    /// </summary>
    [Parameter, EditorRequired]
    public required int FluidMinimumWidth { get; set; }

    /// <summary>
    /// Maximum width of the sidebar when <see cref="Mode"/> is <see cref="SidebarMode.Fluid"/>
    /// </summary>
    [Parameter, EditorRequired]
    public required int FluidMaximumWidth { get; set; }

    /// <summary>
    /// Width of the component when <see cref="Mode"/> is <see cref="SidebarMode.Fluid"/>
    /// </summary>
    [Parameter]
    public int? FluidWidth { get; set; }

    /// <summary>
    /// Raised when <see cref="FluidWidth"/> has changed
    /// </summary>
    [Parameter]
    public EventCallback<int?> FluidWidthChanged { get; set; }

    /// <summary>
    /// Content displayed in the sidebar
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Inject] private IResizeInteraction ResizeInteraction { get; set; } = default!;

    bool IResizeable.Resizeable => Mode == SidebarMode.Fluid;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        _placement = Placement;
        _fluidMinimumWidth = FluidMinimumWidth;
        _fluidMaximumWidth = FluidMaximumWidth;

        EnsureFluidMinimumBelowOrEqualFluidMaximum();
        UpdateWidth();
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        var fluidMinimumWidthChanged = false;
        var fluidMaximumWidthChanged = false;

        if (FluidMinimumWidth != _fluidMinimumWidth)
        {
            _fluidMinimumWidth = FluidMinimumWidth;

            fluidMinimumWidthChanged = true;
        }

        if (FluidMaximumWidth != _fluidMaximumWidth)
        {
            _fluidMaximumWidth = FluidMaximumWidth;

            fluidMaximumWidthChanged = true;
        }

        if (fluidMinimumWidthChanged || fluidMaximumWidthChanged)
            EnsureFluidMinimumBelowOrEqualFluidMaximum();

        // The resize interaction reads the minimum width and the resize handle position once while it is
        // being attached, so it has to be attached anew to pick either of them up.
        if (fluidMinimumWidthChanged || Placement != _placement)
        {
            _placement = Placement;

            _reattachResizeInteraction = true;
        }

        UpdateWidth();
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (_disposedAsync)
            return;

        if (Mode != SidebarMode.Fluid)
        {
            await RemoveResizeInteractionAsync();

            return;
        }

        if (_reattachResizeInteraction)
        {
            await RemoveResizeInteractionAsync();

            _reattachResizeInteraction = false;
        }

        if (_resizeInteractionAttachTask is null && _resizeHandles.Count > 0)
        {
            _resizeInteractionAttachTask = ResizeInteraction.AttachAsync(this);

            await _resizeInteractionAttachTask;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await RemoveResizeInteractionAsync();
    }

    ElementReference IResizeable.GetElementReference()
        => _sidebarElementReference;

    ElementReference IResizeContainer.GetElementReference()
        => _resizeContainerElementReference;

    IReadOnlyCollection<IResizeHandle> IResizeable.GetResizeHandles()
        => _resizeHandles;

    IResizeContainer IResizeable.GetResizeContainer()
        => this;

    double IResizeable.GetMinimumWidth()
        => _fluidMinimumWidth;

    double IResizeable.GetMinimumHeight()
        => 0;

    void IResizeable.RegisterResizeHandle(IResizeHandle resizeHandle)
    {
        if (_resizeHandles.Contains(resizeHandle))
            return;

        _resizeHandles.Add(resizeHandle);

        _reattachResizeInteraction = true;
    }

    void IResizeable.UnregisterResizeHandle(IResizeHandle resizeHandle)
    {
        if (_resizeHandles.Remove(resizeHandle))
            _reattachResizeInteraction = true;
    }

    async Task IResizeable.UpdatePositionAndSizeAsync(DomRect domRect)
    {
        _width = ClampToFluidWidthRange((int)Math.Round(domRect.Width));

        if (FluidWidthChanged.HasDelegate)
            await FluidWidthChanged.InvokeAsync(_width);

        FluidWidth = _width;

        await InvokeAsync(StateHasChanged);
    }

    private async Task RemoveResizeInteractionAsync()
    {
        if (_resizeInteractionAttachTask?.IsCompletedSuccessfully != true)
            return;

        await ResizeInteraction.RemoveAsync(this);

        _resizeInteractionAttachTask = null;
    }

    private void EnsureFluidMinimumBelowOrEqualFluidMaximum()
    {
        if (_fluidMinimumWidth > _fluidMaximumWidth)
            _fluidMinimumWidth = _fluidMaximumWidth;
    }

    private void UpdateWidth()
    {
        if (Mode != SidebarMode.Fluid)
        {
            _width = CompactWidth;

            return;
        }

        if (FluidWidth.HasValue)
            _width = FluidWidth.Value;

        _width = ClampToFluidWidthRange(_width);
    }

    private int ClampToFluidWidthRange(int width)
        => Math.Clamp(width, _fluidMinimumWidth, _fluidMaximumWidth);
}
