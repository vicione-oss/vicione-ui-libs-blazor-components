using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.TabStrip.Components;

namespace ViciOne.Ui.Blazor.Components.TabStrip.Models;

internal record TabContext
{
    internal required ITab Instance { get; init; }
    internal ElementReference? ElementReference { get; set; }
}
