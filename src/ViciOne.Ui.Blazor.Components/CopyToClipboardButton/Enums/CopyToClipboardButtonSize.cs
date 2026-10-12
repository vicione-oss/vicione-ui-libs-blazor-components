using ViciOne.Ui.Blazor.Components.Enums;

namespace ViciOne.Ui.Blazor.Components.CopyToClipboardButton.Enums;

/// <summary>
/// Defines sizes for the <see cref="Components.CopyToClipboardButton"/> component
/// </summary>
public readonly record struct CopyToClipboardButtonSize : ITypeSafeEnumImplemention<CopyToClipboardButtonSize>
{
    /// <summary>
    /// 24 x 24 pixel
    /// </summary>
    public static readonly CopyToClipboardButtonSize Small = new(nameof(Small));

    /// <summary>
    /// 32 x 32 pixel
    /// </summary>
    public static readonly CopyToClipboardButtonSize Medium = new(nameof(Medium));

    /// <summary>
    /// 36 x 36 pixel
    /// </summary>
    public static readonly CopyToClipboardButtonSize Large = new(nameof(Large));

    private readonly string _name;

    internal CopyToClipboardButtonSize(string name)
        => _name = name;

    /// <inheritdoc/>
    public static CopyToClipboardButtonSize GetDefaultValue()
        => Medium;

    /// <inheritdoc/>
    public string GetName()
        => _name;

    /// <inheritdoc cref="GetName"/>
    public override string ToString()
        => GetName();
}
