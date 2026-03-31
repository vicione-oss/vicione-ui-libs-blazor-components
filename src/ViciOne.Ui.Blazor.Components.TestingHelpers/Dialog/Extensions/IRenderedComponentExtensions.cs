using AngleSharp.Dom;
using Bunit;
using Bunit.Rendering;
using Microsoft.AspNetCore.Components.Sections;
using ViciOne.Ui.Blazor.Components.Dialog.Components;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Button.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Popup.Extensions;
using ButtonComponent = ViciOne.Ui.Blazor.Components.Button.Button;
using DialogComponent = ViciOne.Ui.Blazor.Components.Dialog.Components.Dialog;
using PopupComponent = ViciOne.Ui.Blazor.Components.Popup.Components.Popup;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Dialog.Extensions;

/// <summary>
/// Extension methods for <see cref="IRenderedComponent{T}"/> where T is <see cref="DialogComponent"/>.
/// </summary>
public static class IRenderedComponentExtensions
{
    /// <summary>
    /// Renders and returns the content of the <see cref="SectionContent"/> found within the inner usage of <see cref="PopupComponent"/>
    /// in <paramref name="renderedComponent"/>.
    /// </summary>
    public static IRenderedComponent<ContainerFragment> RenderSectionContent(this IRenderedComponent<DialogComponent> renderedComponent,
        BunitContext testContext)
    {
        var popup = renderedComponent.FindComponent<PopupComponent>();

        return popup.RenderSectionContent(testContext);
    }

    /// <summary>
    /// Finds the first <see cref="DialogFooterButton"/> in the provided <paramref name="sectionContent"/> that matches
    /// the specified <paramref name="predicate"/>.
    /// </summary>
	/// <exception cref="Exceptions.ElementNotFoundException">Thrown if no inner button was found.</exception>
    public static IRenderedComponent<DialogFooterButton> FindFooterButton(this IRenderedComponent<ContainerFragment> sectionContent,
        Predicate<ButtonComponent> predicate)
    {
        var footerButtons = sectionContent.FindComponents<DialogFooterButton>();

        foreach (var footerButton in footerButtons)
        {
            var buttons = footerButton.FindComponents<ButtonComponent>();
            if (buttons.Count == 0)
                continue;

            var innerButton = buttons[0];
            if (predicate(innerButton.Instance))
                return footerButton;
        }

        throw new Exceptions.ElementNotFoundException($"Predicate did not match any of the {footerButtons.Count} button(s).");
    }

    /// <inheritdoc cref="Button.Extensions.IRenderedComponentExtensions.FindInnerButton(IRenderedComponent{ButtonComponent})"/>
    public static IElement FindInnerButton(this IRenderedComponent<DialogFooterButton> dialogFooterButton)
    {
        var innerButtonComponent = dialogFooterButton.FindComponent<ButtonComponent>();
        var innerButton = innerButtonComponent.FindInnerButton();

        return innerButton;
    }
}
