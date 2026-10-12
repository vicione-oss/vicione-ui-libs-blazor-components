using Bunit;
using CopyToClipboardButtonComponent = ViciOne.Ui.Blazor.Components.CopyToClipboardButton.Components.CopyToClipboardButton;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.CopyToClipboardButton.Extensions;

/// <summary>
/// Extension methods for <see cref="BunitJSInterop"/>
/// </summary>
public static class BUnitJsInteropExtensions
{
    /// <summary>
    /// Configures handlers for all JS-interop calls a <see cref="CopyToClipboardButtonComponent"/> makes,
    /// including the import of the ripple module, using loose mode for its own module.
    /// </summary>
    /// <returns>
    /// The handler of the copy, whose invocations carry the text to copy as their first argument.
    /// </returns>
    public static JSRuntimeInvocationHandler SetupForCopyToClipboardButton(this BunitJSInterop jsInterop)
    {
        jsInterop.SetupVoid("import", "./_content/ViciOne.Ui.Blazor.Components/material-web/ripple.js").SetVoidResult();

        var jsModule = jsInterop.SetupModule(
            "./_content/ViciOne.Ui.Blazor.Components/copy-to-clipboard-button/components/copy-to-clipboard-button.js");
        jsModule.Mode = JSRuntimeMode.Loose;

        var jsInstance = jsModule.SetupModule("CopyToClipboardButton", _ => true);
        jsInstance.Mode = JSRuntimeMode.Loose;

        return jsInstance.SetupVoid("copy", _ => true);
    }
}
