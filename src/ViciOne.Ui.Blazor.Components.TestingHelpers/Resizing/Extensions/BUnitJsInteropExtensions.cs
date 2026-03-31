using Bunit;
using ViciOne.Ui.Blazor.Components.Resizing.Services;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Resizing.Extensions;

/// <summary>
/// Extension methods for <see cref="BunitJSInterop"/>
/// </summary>
public static class BUnitJsInteropExtensions
{
    /// <summary>
    /// Configures a handler for JS-interop calls associated with <see cref="ResizeObserver"/> using loose mode.
    /// </summary>
    /// <returns>
    /// The bUnit JS-interop module associated with <see cref="ResizeObserver"/>.
    /// </returns>
    public static BunitJSModuleInterop SetupForResizeObserver(this BunitJSInterop jsInterop)
    {
        var jsModule = jsInterop.SetupModule("./_content/ViciOne.Ui.Blazor.Components/resizing/resize-observer.js");
        jsModule.Mode = JSRuntimeMode.Loose;

        return jsModule;
    }
}
