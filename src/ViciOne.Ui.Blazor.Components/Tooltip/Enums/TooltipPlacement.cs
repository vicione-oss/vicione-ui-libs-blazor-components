using ViciOne.Ui.Blazor.Components.Enums;

namespace ViciOne.Ui.Blazor.Components.Tooltip.Enums;

/// <summary>
/// Defines placements of a tooltip relative to the element it belongs to
/// </summary>
public readonly record struct TooltipPlacement : ITypeSafeEnumImplemention<TooltipPlacement>
{
    /// <summary>
    /// Placed above the element
    /// </summary>
    public static readonly TooltipPlacement Top = new(nameof(Top));

    /// <summary>
    /// Placed right of the element
    /// </summary>
    public static readonly TooltipPlacement Right = new(nameof(Right));

    /// <summary>
    /// Placed below the element
    /// </summary>
    public static readonly TooltipPlacement Bottom = new(nameof(Bottom));

    /// <summary>
    /// Placed left of the element
    /// </summary>
    public static readonly TooltipPlacement Left = new(nameof(Left));

    private readonly string _name;

    internal TooltipPlacement(string name)
        => _name = name;

    /// <inheritdoc/>
    public static TooltipPlacement GetDefaultValue()
        => Top;

    /// <inheritdoc/>
    public string GetName()
        => _name;

    /// <inheritdoc cref="GetName"/>
    public override string ToString()
        => GetName();
}
