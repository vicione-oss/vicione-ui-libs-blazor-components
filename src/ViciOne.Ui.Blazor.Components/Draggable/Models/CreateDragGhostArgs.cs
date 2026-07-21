using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Attributes;
using ViciOne.Ui.Blazor.Components.Draggable.Components;

namespace ViciOne.Ui.Blazor.Components.Draggable.Models;

[GenerateTypeScriptClass]
internal sealed record CreateDragGhostArgs
{
    public required DotNetObjectReference<DragGhostBase> DotNetObject { get; init; }
    public required ElementReference ContentElementReference { get; init; }

    /// <summary>
    /// Whether the deriving component implements the drag-start lifecycle listener. The factory exposes the
    /// instance's <c>dragStart</c> callback only when this is <see langword="true"/>.
    /// </summary>
    public required bool ProcessDragStart { get; init; }

    /// <summary>
    /// Whether the deriving component implements the drag-end lifecycle listener. The factory exposes the
    /// instance's <c>dragEnd</c> callback only when this is <see langword="true"/>.
    /// </summary>
    public required bool ProcessDragEnd { get; init; }

    /// <summary>
    /// Whether the deriving component implements the dropzone-enter lifecycle listener. The factory exposes the
    /// instance's <c>dropzoneEnter</c> callback only when this is <see langword="true"/>.
    /// </summary>
    public required bool ProcessDropzoneEnter { get; init; }

    /// <summary>
    /// Whether the deriving component implements the dropzone-leave lifecycle listener. The factory exposes the
    /// instance's <c>dropzoneLeave</c> callback only when this is <see langword="true"/>.
    /// </summary>
    public required bool ProcessDropzoneLeave { get; init; }
}
