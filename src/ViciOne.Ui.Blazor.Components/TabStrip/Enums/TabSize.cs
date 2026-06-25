using ViciOne.Ui.Blazor.Components.Enums;

namespace ViciOne.Ui.Blazor.Components.TabStrip.Enums;

/// <summary>
/// Defines sizes for the <see cref="TabStrip" /> component
/// </summary>
public readonly record struct TabSize : ITypeSafeEnumImplemention<TabSize>
{
    /// <summary>
    /// 27 pixel height
    /// </summary>
    public static readonly TabSize Small = new(nameof(Small));

    /// <summary>
    /// 36 pixel height
    /// </summary>
    public static readonly TabSize Large = new(nameof(Large));

    private readonly string _name;

    internal TabSize(string name)
        => _name = name;

    /// <inheritdoc/>
    public static TabSize GetDefaultValue()
        => Small;

    /// <inheritdoc/>
    public string GetName()
        => _name;

    /// <inheritdoc cref="GetName"/>
    public override string ToString()
        => GetName();
}
