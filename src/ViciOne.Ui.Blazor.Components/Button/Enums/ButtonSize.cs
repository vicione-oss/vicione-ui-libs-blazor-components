using ViciOne.Ui.Blazor.Components.Enums;

namespace ViciOne.Ui.Blazor.Components.Button.Enums;

/// <summary>
/// Defines sizes for the <see cref="Button" /> component
/// </summary>
public readonly record struct ButtonSize : ITypeSafeEnumImplemention<ButtonSize>
{
    /// <summary>
    ///  Height of 24 pixel
    /// </summary>
    public static readonly ButtonSize Small = new(nameof(Small));

    /// <summary>
    ///  Height of 32 pixel
    /// </summary>
    public static readonly ButtonSize Medium = new(nameof(Medium));

    /// <summary>
    ///  Height of 36 pixel
    /// </summary>
    public static readonly ButtonSize Large = new(nameof(Large));

    private readonly string _name;

    internal ButtonSize(string name)
        => _name = name;

    /// <inheritdoc/>
    public static ButtonSize GetDefaultValue()
        => Small;

    /// <inheritdoc/>
    public string GetName()
        => _name;

    /// <inheritdoc cref="GetName"/>
    public override string ToString()
        => GetName();
}
