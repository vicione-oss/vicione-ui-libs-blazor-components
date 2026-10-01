using Microsoft.AspNetCore.Components;

namespace Shared.Components;

public sealed partial class DescriptionBanner : ComponentBase
{
    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; }
}
