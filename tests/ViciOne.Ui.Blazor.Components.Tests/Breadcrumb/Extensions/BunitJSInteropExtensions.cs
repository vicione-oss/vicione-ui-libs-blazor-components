using Bunit;
using ViciOne.Ui.Blazor.Components.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Breadcrumb.Extensions;

internal static class BunitJSInteropExtensions
{
    public static JSRuntimeInvocationHandler<IEnumerable<DomRect>> SetupGetBoundingClientRects(
        this BunitJSInterop jsInterop)
    {
        var module = jsInterop.SetupModule("./_content/ViciOne.Ui.Blazor.Components/breadcrumb/html-element-helper.js");

        return module.Setup<IEnumerable<DomRect>>("getBoundingClientRects", _ => true);
    }
}
