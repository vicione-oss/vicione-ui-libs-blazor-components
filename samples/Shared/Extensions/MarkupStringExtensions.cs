using Microsoft.AspNetCore.Components;

namespace Shared.Extensions;

internal static class MarkupStringExtensions
{
    public static MarkupString MakeBold(this MarkupString str)
        => new($"<b>{str}</b>");

    public static MarkupString MakeItalic(this MarkupString str)
        => new($"<i>{str}</i>");

    public static MarkupString WithHighlightedText(this MarkupString str, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return str;

        return new MarkupString(str.ToString().Replace(text, $"<code>{text}</code>", StringComparison.Ordinal));
    }

}
