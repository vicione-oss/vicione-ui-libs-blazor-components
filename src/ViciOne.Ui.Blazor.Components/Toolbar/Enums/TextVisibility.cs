using ViciOne.Ui.Blazor.Components.Enums;

namespace ViciOne.Ui.Blazor.Components.Toolbar.Enums;

/// <summary>
/// Defines visibility of toolbar item text.
/// </summary>
public readonly record struct TextVisibility : ITypeSafeEnumImplemention<TextVisibility>
{
    /// <summary>
    /// The text is always visible, both in the toolbar and the menu.
    /// </summary>
    public static readonly TextVisibility Always = new(nameof(Always));

    /// <summary>
    /// The text is only visible in the menu, but not in the toolbar.
    /// </summary>
    public static readonly TextVisibility MenuOnly = new(nameof(MenuOnly));

    /// <summary>
    /// The text is only visible in the toolbar, but not in the menu.
    /// </summary>
    public static readonly TextVisibility ToolbarOnly = new(nameof(ToolbarOnly));

    private readonly string _name;

    internal TextVisibility(string name)
        => _name = name;

    /// <inheritdoc/>
    public static TextVisibility GetDefaultValue()
        => Always;

    /// <inheritdoc/>
    public string GetName()
        => _name;

    /// <inheritdoc cref="GetName"/>
    public override string ToString()
        => GetName();
}
