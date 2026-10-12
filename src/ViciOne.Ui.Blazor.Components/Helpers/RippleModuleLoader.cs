using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;

namespace ViciOne.Ui.Blazor.Components.Helpers;

/// <summary>
/// Loads the module defining the <c>md-ripple</c> element once per <see cref="IJSRuntime"/>, for all components rendering it
/// </summary>
internal static class RippleModuleLoader
{
    private static readonly string s_modulePath =
        $"./_content/{typeof(RippleModuleLoader).Assembly.GetName().Name}/material-web/ripple.js";

    private static readonly ConditionalWeakTable<IJSRuntime, Task> s_moduleLoads = [];

    public static Task LoadAsync(IJSRuntime jsRuntime, ILogger logger)
        => s_moduleLoads.GetValue(jsRuntime, runtime => runtime.ImportAsync(s_modulePath, logger));
}
