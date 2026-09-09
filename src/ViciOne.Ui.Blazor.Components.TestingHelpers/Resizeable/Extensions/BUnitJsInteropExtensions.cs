using Bunit;
using ViciOne.Ui.Blazor.Components.Resizeable.Services;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Resizeable.Extensions;

/// <summary>
/// Extension methods for <see cref="BunitJSInterop"/>
/// </summary>
public static class BUnitJsInteropExtensions
{
    /// <summary>
    /// Configures a handler for JS-interop calls associated with <see cref="IResizeInteraction"/> using loose mode.
    /// </summary>
    /// <returns>
    /// The bUnit JS-interop module associated with <see cref="IResizeInteraction"/>.
    /// </returns>
    public static BunitJSModuleInterop SetupForResizeInteraction(this BunitJSInterop jsInterop)
    {
        var jsModule = jsInterop.SetupModule("./_content/ViciOne.Ui.Blazor.Components/resizeable/resize-interaction.js");
        jsModule.Mode = JSRuntimeMode.Loose;

        return jsModule;
    }
}
