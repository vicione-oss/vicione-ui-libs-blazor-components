using ViciOne.Ui.Blazor.Components.Enums;

namespace ViciOne.Ui.Blazor.Components.Switch.Enums;

/// <summary>
/// Defines sizes for the <see cref="Switch" /> component
/// </summary>
public readonly record struct SwitchSize : ITypeSafeEnumImplemention<SwitchSize>
{
    /// <summary>
    /// 24 x 12 pixel
    /// </summary>
    public static readonly SwitchSize Small = new(nameof(Small));

    /// <summary>
    /// 32 x 16 pixel
    /// </summary>
    public static readonly SwitchSize Medium = new(nameof(Medium));

    /// <summary>
    /// 40 x 20 pixel
    /// </summary>
    public static readonly SwitchSize Large = new(nameof(Large));

    private readonly string _name;

    internal SwitchSize(string name)
        => _name = name;

    /// <inheritdoc/>
    public static SwitchSize GetDefaultValue()
        => Small;

    /// <inheritdoc/>
    public string GetName()
        => _name;

    /// <inheritdoc cref="GetName"/>
    public override string ToString()
        => GetName();
}
