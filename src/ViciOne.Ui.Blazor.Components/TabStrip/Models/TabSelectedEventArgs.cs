namespace ViciOne.Ui.Blazor.Components.TabStrip.Models;

/// <summary>
/// Provides data for a tab selection event.
/// </summary>
/// <param name="tabIndex">The zero-based index of the selected tab.</param>
public sealed class TabSelectedEventArgs(int tabIndex) : EventArgs
{
    /// <summary>
    /// Gets the zero-based index of the selected tab.
    /// </summary>
    public int TabIndex { get; } = tabIndex;
}
