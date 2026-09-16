using Microsoft.AspNetCore.Components;

namespace Shared.Pages.Button;

public sealed partial class ButtonPage : ComponentBase
{
    private bool _busy;

    private async Task RunActionAsync()
    {
        _busy = true;

        await Task.Delay(TimeSpan.FromSeconds(3));

        _busy = false;
    }
}
