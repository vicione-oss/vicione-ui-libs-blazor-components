using ViciOne.Ui.Blazor.Components.Enums;

namespace ViciOne.Ui.Blazor.Components.Button.Enums;

/// <summary>
/// Defines how the <see cref="Button" /> component shows that it is busy
/// </summary>
public readonly record struct ButtonBusyIndication : ITypeSafeEnumImplemention<ButtonBusyIndication>
{
    /// <summary>
    /// A light arc sweeping around the border
    /// </summary>
    public static readonly ButtonBusyIndication Sweep = new(nameof(Sweep));

    /// <summary>
    /// The icon rotating, leaving the border untouched
    /// </summary>
    /// <remarks>
    /// A button without an icon has nothing to rotate, so pick this only where an icon is always present.
    /// </remarks>
    public static readonly ButtonBusyIndication SpinningIcon = new(nameof(SpinningIcon));

    /// <summary>
    /// A light arc sweeping around the border while the icon rotates
    /// </summary>
    public static readonly ButtonBusyIndication SweepAndSpinningIcon = new(nameof(SweepAndSpinningIcon));

    private readonly string _name;

    internal ButtonBusyIndication(string name)
        => _name = name;

    /// <inheritdoc/>
    public static ButtonBusyIndication GetDefaultValue()
        => Sweep;

    /// <inheritdoc/>
    public string GetName()
        => _name;

    /// <inheritdoc cref="GetName"/>
    public override string ToString()
        => GetName();
}
