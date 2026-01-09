using System.Security.Cryptography;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tooltip.Services;

namespace Shared.Pages.Tooltip.Components;

public sealed partial class TooltipPage
{
    private readonly int _random = RandomNumberGenerator.GetInt32(0, 2);
    private readonly int _tableRows = RandomNumberGenerator.GetInt32(5, 25);

    [Inject] private TooltipService Service { get; set; } = default!;
}
