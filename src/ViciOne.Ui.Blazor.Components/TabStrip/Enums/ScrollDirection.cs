using ViciOne.Ui.Blazor.Components.Enums;

namespace ViciOne.Ui.Blazor.Components.TabStrip.Enums;

/// <summary>
/// Defines the scroll direction for a <see cref="Components.TabStripScrollButton"/>.
/// </summary>
public readonly record struct ScrollDirection : ITypeSafeEnumImplemention<ScrollDirection>
{
    /// <summary>
    /// Scroll to the left.
    /// </summary>
    public static readonly ScrollDirection Left = new(nameof(Left));

    /// <summary>
    /// Scroll to the right.
    /// </summary>
    public static readonly ScrollDirection Right = new(nameof(Right));

    private readonly string _name;

    internal ScrollDirection(string name)
        => _name = name;

    /// <inheritdoc/>
    public static ScrollDirection GetDefaultValue()
        => Left;

    /// <inheritdoc/>
    public string GetName()
        => _name;

    /// <inheritdoc cref="GetName"/>
    public override string ToString()
        => GetName();
}
