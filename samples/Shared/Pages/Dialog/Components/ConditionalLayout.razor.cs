using Microsoft.AspNetCore.Components;

namespace Shared.Pages.Dialog.Components;

public sealed partial class ConditionalLayout
{
    [Parameter, EditorRequired] public bool Condition { get; set; }
    [Parameter, EditorRequired] public RenderFragment<RenderFragment> Layout { get; set; }
    [Parameter, EditorRequired] public RenderFragment Content { get; set; }
}
