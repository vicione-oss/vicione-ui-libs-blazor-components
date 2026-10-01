namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Filters;

/// <summary>
/// Viewport coordinates of the filter trigger button, measured in JavaScript. The panel is placed at
/// <see cref="Left"/>/<see cref="Bottom"/> so it opens flush under the button.
/// </summary>
/// <param name="Left">Distance from the left viewport edge to the button's left edge, in pixels.</param>
/// <param name="Bottom">Distance from the top viewport edge to the button's bottom edge, in pixels.</param>
internal readonly record struct PanelPosition(double Left, double Bottom);
