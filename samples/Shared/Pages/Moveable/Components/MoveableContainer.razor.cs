using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Moveable.Interfaces;

namespace Shared.Pages.Moveable.Components;

public sealed partial class MoveableContainer : IMoveContainer
{
    private ElementReference _elementReference;

    [Parameter] public int GridSize { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    public ElementReference GetElementReference() => _elementReference;
}
