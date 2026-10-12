using Microsoft.AspNetCore.Components;

namespace Shared.Pages.CopyToClipboardButton;

public sealed partial class CopyToClipboardButtonPage : ComponentBase
{
    private string? _text = "Copied by the button";

    private Task<string> GetTextToCopyAsync()
        => Task.FromResult(_text ?? string.Empty);
}
