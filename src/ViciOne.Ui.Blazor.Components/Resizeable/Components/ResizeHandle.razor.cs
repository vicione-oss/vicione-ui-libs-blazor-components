using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Resizeable.Enums;

namespace ViciOne.Ui.Blazor.Components.Resizeable.Components;

/// <summary>
/// A component that acts as a resize handle in a resize interaction.
/// </summary>
public sealed partial class ResizeHandle : ComponentBase, IResizeHandle, IDisposable
{
    private ElementReference _elementReference;
    private string _positionCssClass = string.Empty;

    /// <summary>
    /// Gets or sets the resizeable that registers this resize handle.
    /// </summary>
    [CascadingParameter]
    public IResizeable Resizeable { get; set; } = default!;

    /// <inheritdoc/>
    [Parameter, EditorRequired]
    public ResizeHandlePosition Position { get; set; }

    private string PositionCssClass => _positionCssClass;

    /// <inheritdoc/>
    protected override void OnParametersSet()
        => _positionCssClass = Position.ToString().ToDashCase();

    /// <inheritdoc/>
    protected override void OnInitialized()
        => Resizeable.RegisterResizeHandle(this);

    /// <inheritdoc/>
    public void Dispose()
        => Resizeable.UnregisterResizeHandle(this);

    /// <inheritdoc/>
    public ElementReference GetElementReference()
        => _elementReference;
}
