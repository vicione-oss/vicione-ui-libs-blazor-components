namespace ViciOne.Ui.Blazor.Components.Models;

/// <summary>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/DOMRect"/>
/// </summary>
public sealed record DomRect
{
    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/DOMRect/width"/>
    /// </summary>
    public double Width { get; init; }

    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/DOMRect/height"/>
    /// </summary>
    public double Height { get; init; }
}
