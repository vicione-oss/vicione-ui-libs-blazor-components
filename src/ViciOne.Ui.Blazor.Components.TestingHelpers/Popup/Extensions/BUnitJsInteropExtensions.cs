using Bunit;
using PopupComponent = ViciOne.Ui.Blazor.Components.Popup.Components.Popup;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Popup.Extensions;

/// <summary>
/// Extension methods for <see cref="BunitJSInterop"/>
/// </summary>
public static class BUnitJsInteropExtensions
{
    /// <summary>
    /// Configures handlers for all JS-interop calls a <see cref="PopupComponent"/> makes, using loose mode. These are
    /// the calls to trap the focus and to close on 'Escape'.
    /// </summary>
    /// <returns>
    /// The bUnit JS-interop module associated with the focus trap of a <see cref="PopupComponent"/>.
    /// </returns>
    public static BunitJSModuleInterop SetupForPopup(this BunitJSInterop jsInterop)
    {
        var popupJsModule = jsInterop.SetupModule("./_content/ViciOne.Ui.Blazor.Components/popup/components/popup.js");
        popupJsModule.Mode = JSRuntimeMode.Loose;
        popupJsModule.SetupModule("Popup", _ => true);

        var jsModule = jsInterop.SetupModule("./_content/ViciOne.Ui.Blazor.Components/focus-trap/focus-trap.js");
        jsModule.Mode = JSRuntimeMode.Loose;
        jsModule.SetupModule("FocusTrap", _ => true);

        return jsModule;
    }
}
