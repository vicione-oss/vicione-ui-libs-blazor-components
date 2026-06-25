using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Resizeable.Components;

namespace Shared.Pages.Resizeable.Components;

public sealed partial class ResizeableContainer : IResizeContainer
{
    private ElementReference _elementReference;

    [Parameter] public int GridSize { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    public ElementReference GetElementReference() => _elementReference;
}
