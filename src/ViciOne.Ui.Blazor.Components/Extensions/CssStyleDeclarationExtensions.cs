using ViciOne.Ui.Blazor.Components.Models;

namespace ViciOne.Ui.Blazor.Components.Extensions;

internal static class CssStyleDeclarationExtensions
{
    public static bool NearlyEquals(this CssStyleDeclaration cssStyleDeclaration, CssStyleDeclaration other)
        => cssStyleDeclaration.MarginLeft.NearlyEquals(other.MarginLeft) &&
            cssStyleDeclaration.MarginRight.NearlyEquals(other.MarginRight) &&
            cssStyleDeclaration.MarginTop.NearlyEquals(other.MarginTop) &&
            cssStyleDeclaration.MarginBottom.NearlyEquals(other.MarginBottom) &&
            cssStyleDeclaration.PaddingLeft.NearlyEquals(other.PaddingLeft) &&
            cssStyleDeclaration.PaddingRight.NearlyEquals(other.PaddingRight) &&
            cssStyleDeclaration.PaddingTop.NearlyEquals(other.PaddingTop) &&
            cssStyleDeclaration.PaddingBottom.NearlyEquals(other.PaddingBottom) &&
            cssStyleDeclaration.BorderLeft.NearlyEquals(other.BorderLeft) &&
            cssStyleDeclaration.BorderRight.NearlyEquals(other.BorderRight) &&
            cssStyleDeclaration.BorderTop.NearlyEquals(other.BorderTop) &&
            cssStyleDeclaration.BorderBottom.NearlyEquals(other.BorderBottom);
}
