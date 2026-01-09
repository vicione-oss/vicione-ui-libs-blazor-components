namespace ViciOne.Ui.Blazor.Components.LoadingSpinner.Models;

/// <summary>
/// Render context for a <see cref="Components.LoadingSpinner"/>
/// </summary>
public sealed class RenderContext
{
    /// <summary>
    /// Message that should be rendered
    /// </summary>
    public required MessageRenderContext Message { get; init; }
}
