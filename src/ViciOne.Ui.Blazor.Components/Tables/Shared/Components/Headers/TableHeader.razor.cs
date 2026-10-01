using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Components.Headers;

/// <summary>
/// Table header with distinct regions for action buttons and data filtering.
/// </summary>
public sealed partial class TableHeader : ComponentBase
{
    /// <summary>
    /// The content displayed in the button region of the header.
    /// </summary>
    [Parameter]
    public RenderFragment? ActionButtons { get; set; }

    /// <summary>
    /// The content displayed in the filter region of the header.
    /// </summary>
    [Parameter]
    public RenderFragment? Filter { get; set; }
}
