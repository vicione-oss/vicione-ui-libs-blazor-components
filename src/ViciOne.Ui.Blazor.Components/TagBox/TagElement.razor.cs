using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.TagBox;

/// <summary>
/// Component to provide a tag element in <see cref="TagBox"/>
/// </summary>
public partial class TagElement
{
    /// <summary>
    /// The text of the tag
    /// </summary>
    [Parameter, EditorRequired]
    public required string Tag { get; set; }

    /// <summary>
    /// True when user can remove the tag, otherwise false
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Raised when the delete button was clicked
    /// </summary>
    [Parameter]
    public EventCallback<string> OnDeleteButtonClick { get; set; }
}
