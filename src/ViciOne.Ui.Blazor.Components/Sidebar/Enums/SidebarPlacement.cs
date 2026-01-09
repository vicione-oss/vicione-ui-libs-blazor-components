using ViciOne.Ui.Blazor.Components.Enums;

namespace ViciOne.Ui.Blazor.Components.Sidebar.Enums;

/// <summary>
/// Defines placements of <see cref="Sidebar"/> in an outer container
/// </summary>
public readonly record struct SidebarPlacement : ITypeSafeEnumImplemention<SidebarPlacement>
{
    /// <summary>
    /// Placed on left side of the outer container
    /// </summary>
    public static readonly SidebarPlacement Left = new(nameof(Left));

    /// <summary>
    /// Placed on right side of the outer container
    /// </summary>
    public static readonly SidebarPlacement Right = new(nameof(Right));

    private readonly string _name;

    internal SidebarPlacement(string name)
        => _name = name;

    /// <inheritdoc/>
    public static SidebarPlacement GetDefaultValue()
        => Left;

    /// <inheritdoc/>
    public string GetName()
        => _name;

    /// <inheritdoc cref="GetName"/>
    public override string ToString()
        => GetName();
}
