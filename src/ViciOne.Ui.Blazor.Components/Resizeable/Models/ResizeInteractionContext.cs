using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Attributes;
using ViciOne.Ui.Blazor.Components.Resizeable.Services;

namespace ViciOne.Ui.Blazor.Components.Resizeable.Models;

[GenerateTypeScriptImport(Type = "PointerCaptureBehavior",
    ModulePath = "/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js")]
[GenerateTypeScriptImport(Type = "ResizeHandleInfo",
    ModulePath = "/_content/ViciOne.Ui.Blazor.Components/resizeable/models/resize-handle-info.js")]
[GenerateTypeScriptClass]
internal sealed class ResizeInteractionContext
{
    public required Guid ResizeableId { get; init; }
    public required ElementReference Resizeable { get; init; }

    [TypeScriptPropertyInfo(Type = "ResizeHandleInfo[]")]
    public required ResizeHandleInfo[] ResizeHandles { get; init; }

    public required ElementReference ResizeContainer { get; init; }
    public required double MinimumWidth { get; init; }
    public required double MinimumHeight { get; init; }
    public required string StartedCssClass { get; init; }
    public required string OngoingCssClass { get; init; }
    public required string EndedCssClass { get; init; }
    public required DotNetObjectReference<ResizeInteraction> DotNetObject { get; init; }

    [TypeScriptPropertyInfo(Type = "PointerCaptureBehavior[]")]
    public IJSObjectReference[]? PointerCaptureBehaviors { get; set; }
}
