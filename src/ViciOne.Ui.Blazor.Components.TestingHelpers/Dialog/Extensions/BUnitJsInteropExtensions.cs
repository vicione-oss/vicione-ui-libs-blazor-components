using Bunit;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Popup.Extensions;
using DialogComponent = ViciOne.Ui.Blazor.Components.Dialog.Components.Dialog;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Dialog.Extensions;

/// <summary>
/// Extension methods for <see cref="BunitJSInterop"/>
/// </summary>
public static class BUnitJsInteropExtensions
{
    /// <summary>
    /// Configures handlers for all JS-interop calls a <see cref="DialogComponent"/> makes, using loose mode. These are
    /// the calls to trap the focus and to close on 'Escape'.
    /// </summary>
    /// <returns>
    /// The bUnit JS-interop module associated with the focus trap of a <see cref="DialogComponent"/>.
    /// </returns>
    public static BunitJSModuleInterop SetupForDialog(this BunitJSInterop jsInterop)
        => jsInterop.SetupForPopup();
}
