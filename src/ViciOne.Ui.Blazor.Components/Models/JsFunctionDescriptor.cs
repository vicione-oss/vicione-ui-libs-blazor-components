using ViciOne.Ui.Blazor.Components.Attributes;

namespace ViciOne.Ui.Blazor.Components.Models;

/// <summary>
/// Descriptor for a function implemented on the JavaScript-side.
/// </summary>
[GenerateTypeScriptClass]
public sealed record JsFunctionDescriptor
{
    /// <summary>
    /// Name of the JavaScript function.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Arguments passed to the JavaScript function.
    /// </summary>
    public object? Args { get; init; }
}
