using Bunit;
using Bunit.Rendering;
using Microsoft.AspNetCore.Components.Sections;
using PopupComponent = ViciOne.Ui.Blazor.Components.Popup.Components.Popup;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Popup.Extensions;

/// <summary>
/// Extension methods for <see cref="IRenderedComponent{T}"/> where T is <see cref="PopupComponent"/>.
/// </summary>
public static class IRenderedComponentExtensions
{
    /// <summary>
    /// Renders and returns the content of the <see cref="SectionContent"/> found within the provided <paramref name="renderedComponent"/>.
    /// </summary>
    public static IRenderedComponent<ContainerFragment> RenderSectionContent(this IRenderedComponent<PopupComponent> renderedComponent,
        BunitContext testContext)
    {
        var sectionContent = renderedComponent.FindComponent<SectionContent>();
        var childContent = testContext.Render(sectionContent.Instance.ChildContent!);

        return childContent;
    }
}
