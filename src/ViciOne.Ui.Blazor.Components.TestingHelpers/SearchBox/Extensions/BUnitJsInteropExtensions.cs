using Bunit;
using ViciOne.Ui.Blazor.Components.TestingHelpers.TextBox.Extensions;
using SearchBoxComponent = ViciOne.Ui.Blazor.Components.SearchBox.SearchBox;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.SearchBox.Extensions;

/// <summary>
/// Extension methods for <see cref="BunitJSInterop"/>
/// </summary>
public static class BUnitJsInteropExtensions
{
    /// <summary>
    /// Configures handlers for all JS-interop calls a <see cref="SearchBoxComponent"/> makes, using loose mode.
    /// </summary>
    /// <returns>
    /// The bUnit JS-interop module associated with the input of a <see cref="SearchBoxComponent"/>.
    /// </returns>
    public static BunitJSModuleInterop SetupForSearchBox(this BunitJSInterop jsInterop)
        => jsInterop.SetupForTextBox();
}
