using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Sidebar.Enums;

namespace ViciOne.Ui.Blazor.Components.Sidebar;

/// <summary>
/// A component for rendering a sidebar with support for a mover element to allow adjusting the width of the sidebar via drag.
/// The mover element is rendered when <see cref="Mode"/> is set to <see cref="SidebarMode.Fluid"/>.
/// It covers the box shadow and leaks slightly into the content area to allow for easier dragging.
/// </summary>
/// <remarks>
/// https://en.wikipedia.org/wiki/Sidebar_(computing)
/// </remarks>
public sealed partial class Sidebar : ComponentBase
{
    private int _width;
    private int _fluidMinimumWidth;
    private int _fluidMaximumWidth;
    private double _lastGhostMoveX;
    private bool _changingWidth;

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

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();

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

        UpdateWidth();
    }

    private void OnMoverGhostPointerMove(MouseEventArgs e)
    {
        if (!_changingWidth)
            return;

        var newWidth = _width;

        if (Placement == SidebarPlacement.Left)
            newWidth += (int)(e.ClientX - _lastGhostMoveX);
        else
            newWidth -= (int)(e.ClientX - _lastGhostMoveX);

        _width = Math.Max(Math.Min(_fluidMaximumWidth, newWidth), _fluidMinimumWidth);
        _lastGhostMoveX = e.ClientX;
    }

    private async Task OnMoverGhostPointerUpAsync()
    {
        _changingWidth = false;

        if (FluidWidthChanged.HasDelegate)
            await FluidWidthChanged.InvokeAsync(_width);

        FluidWidth = _width;
    }

    private void OnMoverPointerDown(MouseEventArgs e)
    {
        _changingWidth = true;
        _lastGhostMoveX = e.ClientX;
    }

    private void EnsureFluidMinimumBelowOrEqualFluidMaximum()
    {
        if (_fluidMinimumWidth > _fluidMaximumWidth)
            _fluidMinimumWidth = _fluidMaximumWidth;
    }

    private void UpdateWidth()
    {
        if (Mode == SidebarMode.Fluid)
        {
            if (FluidWidth.HasValue)
                _width = FluidWidth.Value;

            if (_width < _fluidMinimumWidth)
                _width = _fluidMinimumWidth;

            if (_width > _fluidMaximumWidth)
                _width = _fluidMaximumWidth;
        }
        else
        {
            _width = CompactWidth;
        }
    }
}
