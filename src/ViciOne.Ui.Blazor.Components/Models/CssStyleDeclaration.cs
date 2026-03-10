namespace ViciOne.Ui.Blazor.Components.Models;

/// <summary>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleDeclaration"/>
/// </summary>
public sealed record CssStyleDeclaration
{
    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/margin-left"/>
    /// </summary>
    public double MarginLeft { get; init; }

    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/margin-right"/>
    /// </summary>
    public double MarginRight { get; init; }

    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/margin-top"/>
    /// </summary>
    public double MarginTop { get; init; }

    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/margin-bottom"/>
    /// </summary>
    public double MarginBottom { get; init; }

    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/padding-left"/>
    /// </summary>
    public double PaddingLeft { get; init; }

    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/paddingright"/>
    /// </summary>
    public double PaddingRight { get; init; }

    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/padding-top"/>
    /// </summary>
    public double PaddingTop { get; init; }

    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/padding-bottom"/>
    /// </summary>
    public double PaddingBottom { get; init; }

    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/border-left"/>
    /// </summary>
    public double BorderLeft { get; init; }

    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/border-right"/>
    /// </summary>
    public double BorderRight { get; init; }

    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/border-top"/>
    /// </summary>
    public double BorderTop { get; init; }

    /// <summary>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/border-bottom"/>
    /// </summary>
    public double BorderBottom { get; init; }
}
