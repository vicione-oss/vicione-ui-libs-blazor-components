using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Popup.Components;

/// <summary>
/// Component to provide a section in <see cref="PopupRoot"/> for rendering a <see cref="IPopup"/>
/// </summary>
public sealed partial class PopupCell : ComponentBase
{
    /// <summary>
    /// Gets or sets the ID of the provided section for rendering a <see cref="IPopup"/>
    /// </summary>
    [Parameter, EditorRequired]
    public required object SectionId { get; set; }
}
