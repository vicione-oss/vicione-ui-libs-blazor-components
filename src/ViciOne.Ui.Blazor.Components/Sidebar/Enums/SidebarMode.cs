using ViciOne.Ui.Blazor.Components.Enums;

namespace ViciOne.Ui.Blazor.Components.Sidebar.Enums;

/// <summary>
/// Defines the mode of <see cref="Sidebar"/>
/// </summary>
public readonly record struct SidebarMode : ITypeSafeEnumImplemention<SidebarMode>
{
    /// <summary>
    /// Width of the sidebar will be set to <see cref="Sidebar.CompactWidth"/>, no resize handle will be rendered
    /// </summary>
    public static readonly SidebarMode Compact = new(nameof(Compact));

    /// <summary>
    /// Width of the sidebar is adjustable by the resize handle in range from <see cref="Sidebar.FluidMinimumWidth"/>
    /// to <see cref="Sidebar.FluidMaximumWidth"/>
    /// </summary>
    public static readonly SidebarMode Fluid = new(nameof(Fluid));

    private readonly string _name;

    internal SidebarMode(string name)
        => _name = name;

    /// <inheritdoc/>
    public static SidebarMode GetDefaultValue()
        => Compact;

    /// <inheritdoc/>
    public string GetName()
        => _name;

    /// <inheritdoc cref="GetName"/>
    public override string ToString()
        => GetName();
}
