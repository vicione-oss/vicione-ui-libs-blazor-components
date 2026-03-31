using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Dialog.Components;

/// <summary>
/// General layout for dialog bodies containing text.
/// </summary>
public sealed partial class DialogBodyTextLayout : ComponentBase
{
    /// <summary>
    /// Expected to render text.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
