using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Attributes;

namespace ViciOne.Ui.Blazor.Components.Draggable.Models;

/// <summary>
/// Descriptor passed to JS to identify a dropzone by element reference.
/// </summary>
[GenerateTypeScriptClass]
public sealed class DropzoneDescriptor
{
    /// <summary>
    /// Id of the dropzone
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Element reference of the dropzone
    /// </summary>
    public required ElementReference Element { get; init; }
}
