using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.ExpandableMenu.Models;

/// <summary>
/// Model for expandable menu entries
/// </summary>
public sealed class ExpandableMenuEntry : IHasIcon
{
    /// <summary>
    /// Parameters passed to <see cref="DynamicComponent"/> when rendering <see cref="ContentType"/>
    /// </summary>
    public Dictionary<string, object>? ContentParameters { get; init; }

    /// <summary>
    /// Specifies the component type that renders the content of an expanded menu entry
    /// </summary>
    public required Type ContentType { get; set; }

    /// <inheritdoc/>
    public string? IconCssClass { get; set; }

    /// <inheritdoc/>
    public Uri? IconUrl { get; set; }

    /// <inheritdoc/>
    public string? IconData { get; set; }

    /// <summary>
    /// True when the menu entry should be expanded by default, otherwise false
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// True when the menu entry is expanded, otherwise false
    /// </summary>
    internal bool IsExpanded { get; set; }

    /// <summary>
    /// True when the menu entry should stick to the bottom area, otherwise false
    /// </summary>
    public bool IsSticky { get; set; }

    /// <summary>
    /// Text displayed next to the <see cref="IconCssClass">icon</see>
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// True when the menu entry should be visible, otherwise false
    /// </summary>
    public bool Visible { get; set; } = true;
}
