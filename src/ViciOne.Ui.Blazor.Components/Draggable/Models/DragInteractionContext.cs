using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Attributes;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Enums;

namespace ViciOne.Ui.Blazor.Components.Draggable.Models;

[GenerateTypeScriptImport(Type = "PointerCaptureBehavior",
    ModulePath = "../../PointerCapture/Scripts/PointerCaptureBehavior.js")]
[GenerateTypeScriptImport(Type = "ModifierKey",
    ModulePath = "../../Enums/ModifierKey.cs.js")]
[GenerateTypeScriptImport(Type = "DragGhostJsModuleDescriptor",
    ModulePath = "./DragGhostJsModuleDescriptor.cs.js")]
[GenerateTypeScriptClass]
internal sealed class DragInteractionContext
{
    public required Guid DraggableId { get; init; }
    public required ElementReference Draggable { get; init; }
    public required string StartedCssClass { get; init; }
    public required string OngoingCssClass { get; init; }
    public required string EndedCssClass { get; init; }
    public ModifierKey? ModifierKey { get; init; }
    public required DotNetObjectReference<DragInteraction> DotNetObject { get; init; }

    [TypeScriptPropertyInfo(Type = "PointerCaptureBehavior[]")]
    public IJSObjectReference[]? PointerCaptureBehaviors { get; set; }

    [TypeScriptPropertyInfo(Type = "DragGhostJsModuleDescriptor")]
    public DragGhostJsModuleDescriptor? DragGhostJsModule { get; set; }
}
