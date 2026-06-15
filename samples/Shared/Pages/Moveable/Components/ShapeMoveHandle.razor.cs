using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Moveable.Components;

namespace Shared.Pages.Moveable.Components;

public sealed partial class ShapeMoveHandle : ComponentBase, IMoveHandle, IDisposable
{
    private ElementReference _elementReference;

    [Parameter, EditorRequired]
    public IMoveableShape Shape { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
        => Shape.RegisterMoveHandle(this);

    /// <inheritdoc/>
    public void Dispose()
        => Shape.UnregisterMoveHandle(this);

    /// <inheritdoc/>
    public ElementReference GetElementReference()
        => _elementReference;
}
