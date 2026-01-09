namespace ViciOne.Ui.Blazor.Components.Interfaces;

/// <summary>
/// Describes a component that renders an icon
/// </summary>
public interface IHasIcon
{
    /// <summary>
    /// CSS class that defines the icon
    /// </summary>
    /// <remarks>
    /// This property has priority over <see cref="IconUrl"/>.
    /// </remarks>
    string? IconCssClass { get; }

    /// <summary>
    /// Url of the icon
    /// </summary>
    /// <remarks>
    /// This property has lower priority than <see cref="IconCssClass"/>.
    /// </remarks>
    Uri? IconUrl { get; }

    /// <summary>
    /// https://en.wikipedia.org/wiki/Data_URI_scheme
    /// </summary>
    /// <remarks>
    /// This property has lower priority than <see cref="IconUrl"/>.
    /// </remarks>
    string? IconData { get; }
}
