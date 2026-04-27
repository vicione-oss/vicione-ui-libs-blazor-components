namespace ViciOne.Ui.Blazor.Components.Interfaces;

/// <summary>
/// Describes a component that renders an icon
/// </summary>
public interface IHasIcon
{
    /// <summary>
    /// CSS class that defines the icon.
    ///
    /// <para>
    /// This property has priority over <see cref="IconUrl"/>.
    /// </para>
    /// </summary>
    string? IconCssClass { get; }

    /// <summary>
    /// Url of the icon.
    ///
    /// <para>
    /// This property has lower priority than <see cref="IconCssClass"/>.
    /// </para>
    /// </summary>
    Uri? IconUrl { get; }

    /// <summary>
    /// https://en.wikipedia.org/wiki/Data_URI_scheme
    ///
    /// <para>
    /// This property has lower priority than <see cref="IconUrl"/>.
    /// </para>
    /// </summary>
    string? IconData { get; }
}
