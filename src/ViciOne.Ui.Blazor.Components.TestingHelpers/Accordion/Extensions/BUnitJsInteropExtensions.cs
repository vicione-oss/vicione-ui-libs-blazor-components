using Bunit;
using ViciOne.Ui.Blazor.Components.Accordion.Components;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Accordion.Extensions;

/// <summary>
/// Extension methods for <see cref="BunitJSInterop"/>
/// </summary>
public static class BUnitJsInteropExtensions
{
    /// <summary>
    /// Configures handlers for all JS-interop calls an <see cref="AccordionItem"/> makes, using loose mode. These are
    /// the calls of <see cref="AccordionItem.ScrollIntoViewAsync()"/>.
    /// </summary>
    /// <returns>
    /// The bUnit JS-interop module associated with an <see cref="AccordionItem"/>.
    /// </returns>
    public static BunitJSModuleInterop SetupForAccordionItem(this BunitJSInterop jsInterop)
    {
        var jsModule = jsInterop.SetupModule("./_content/ViciOne.Ui.Blazor.Components/accordion/components/accordion-item.js");
        jsModule.Mode = JSRuntimeMode.Loose;

        return jsModule;
    }
}
