using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Attributes;
using ViciOne.Ui.Blazor.Components.Moveable.Services;

namespace ViciOne.Ui.Blazor.Components.Moveable.Models;

[GenerateTypeScriptClass]
internal sealed class MoveInteractionContext
{
    public required Guid MoveableId { get; init; }
    public required ElementReference Moveable { get; init; }
    public required ElementReference MoveHandle { get; init; }
    public required ElementReference MoveContainer { get; init; }
    public required string StartedCssClass { get; init; }
    public required string EndedCssClass { get; init; }
    public required DotNetObjectReference<MoveInteraction> DotNetObject { get; init; }
}
