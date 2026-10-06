using Bunit;
using TextBoxComponent = ViciOne.Ui.Blazor.Components.TextBox.TextBox;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.TextBox.Extensions;

/// <summary>
/// Extension methods for <see cref="BunitJSInterop"/>
/// </summary>
public static class BUnitJsInteropExtensions
{
    /// <summary>
    /// Configures handlers for all JS-interop calls a <see cref="TextBoxComponent"/> makes, using loose mode.
    /// </summary>
    /// <returns>
    /// The bUnit JS-interop module associated with a <see cref="TextBoxComponent"/>.
    /// </returns>
    public static BunitJSModuleInterop SetupForTextBox(this BunitJSInterop jsInterop)
    {
        var jsModule = jsInterop.SetupModule("./_content/ViciOne.Ui.Blazor.Components/text-box/text-box.js");
        jsModule.Mode = JSRuntimeMode.Loose;
        jsModule.SetupModule("TextBox", _ => true);

        return jsModule;
    }
}
