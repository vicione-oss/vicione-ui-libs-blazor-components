using ViciOne.Ui.Blazor.Components.Attributes;
using ViciOne.Ui.Blazor.Components.Models;

namespace ViciOne.Ui.Blazor.Components.Draggable.Models;

/// <summary>
/// Describes the JavaScript module whose exported factory function creates the drag ghost implementation.
/// </summary>
/// <remarks>
/// The module exposes a single central factory that returns one stateful drag ghost instance. That
/// instance composes the required content detail with whichever lifecycle details it actually implements, so the
/// JavaScript side feature-detects the opt-in callbacks on the instance rather than relying on flags here. An
/// unimplemented callback is simply not exposed and makes no round-trip.
/// </remarks>
[GenerateTypeScriptImport(Type = "JsFunctionDescriptor",
    ModulePath = "/_content/ViciOne.Ui.Blazor.Components/js/js-function-descriptor.js")]
[GenerateTypeScriptClass]
public sealed record DragGhostJsModuleDescriptor
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/import#modulename
    /// </summary>
    public required string ModuleName { get; init; }

    /// <summary>
    /// The descriptor for the exported factory function that creates the single stateful drag ghost instance.
    /// </summary>
    public required JsFunctionDescriptor CreateFunction { get; init; }
}
