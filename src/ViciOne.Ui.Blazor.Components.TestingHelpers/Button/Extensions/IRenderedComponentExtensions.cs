using AngleSharp.Dom;
using Bunit;
using ButtonComponent = ViciOne.Ui.Blazor.Components.Button.Button;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Button.Extensions;

/// <summary>
/// Extension methods for <see cref="IRenderedComponent{T}"/> where T is <see cref="ButtonComponent"/>.
/// </summary>
public static class IRenderedComponentExtensions
{
    /// <summary>
    /// Gets the inner button (the actual HTML button) of the specified <paramref name="renderedComponent"/>.
    /// </summary>
	/// <exception cref="ElementNotFoundException">Thrown if no inner button was found.</exception>
    public static IElement FindInnerButton(this IRenderedComponent<ButtonComponent> renderedComponent)
    {
        var innerButton = renderedComponent.Find(".button");

        return innerButton;
    }
}
