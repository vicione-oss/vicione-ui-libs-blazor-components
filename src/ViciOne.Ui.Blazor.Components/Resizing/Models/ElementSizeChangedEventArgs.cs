using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Services;

namespace ViciOne.Ui.Blazor.Components.Resizing.Models;

/// <summary>
/// Arguments for <see cref="IResizeObserver.ElementSizeChanged"/> or <see cref="IResizeObserver.ElementSizeChangedAsync"/>
/// </summary>
public sealed class ElementSizeChangedEventArgs : EventArgs
{
    /// <summary>
    /// Reference to the element observed for size changes.
    /// </summary>
    public required ElementReference ElementReference { get; init; }

    /// <inheritdoc cref="DomRect"/>
    public required DomRect DomRect { get; init; }

    /// <inheritdoc cref="CssStyleDeclaration"/>
    public CssStyleDeclaration? Style { get; init; }
}
