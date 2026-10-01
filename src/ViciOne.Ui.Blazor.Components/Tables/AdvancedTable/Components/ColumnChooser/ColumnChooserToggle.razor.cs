using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Popup.Components;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.ColumnChooser;

/// <summary>
/// Renders a toggle button and popup chrome for the <see cref="ColumnChooserContent{TItem}"/>.
///
/// <para>
/// The consumer of this component is expected to provide a toggle button implementation
/// in <see cref="Button"/> incorporating the <see cref="ColumnChooserButtonContext"/>
/// to assign the <see cref="ColumnChooserButtonContext.Toggle"/> event callback to the button.
/// </para>
///
/// <para>
/// When the user clicks the button, the toggle component will be notified and the popup
/// will be displayed.
/// </para>
/// </summary>
public sealed partial class ColumnChooserToggle : ComponentBase, IPopup
{
    private bool _popupVisible;

    /// <summary>
    /// The unique identifier of the toggle.
    /// </summary>
    /// <remarks>
    /// It is used internally to bridge the gap between toggle and table component,
    /// hence it must match the <c>ColumnChooserToggleId</c> given to the table.
    /// </remarks>
    [Parameter, EditorRequired]
    public required object Id { get; set; }

    /// <summary>
    /// Direction in which the popup is opened.
    /// The other boundary always is aligned with the button.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="false"/>, causing opening to the right.
    /// </remarks>
    [Parameter]
    public bool OpenPopupToLeft { get; set; }

    /// <summary>
    /// Maximum number of columns listed in the popup at once.
    /// Further columns are reached by scrolling the list.
    /// </summary>
    /// <remarks>
    /// Defaults to 10. Values below 1 are treated as 1.
    /// </remarks>
    [Parameter]
    public int MaximumVisibleRowCount { get; set; } = 10;

    /// <summary>
    /// Renders the button that opens and closes the popup.
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment<ColumnChooserButtonContext> Button { get; set; }

    private static string PopupTitle => Localization.ColumnChooserToggle.PopupTitle;

    // A value below 1 would collapse the list and leave no column reachable, so the list shows at least one row.
    private int EffectiveMaximumVisibleRowCount => Math.Max(MaximumVisibleRowCount, 1);

    private void TogglePopup()
        => _popupVisible = !_popupVisible;

    /// <inheritdoc/>
    async Task IPopup.CloseAsync()
    {
        _popupVisible = false;

        await InvokeAsync(StateHasChanged);
    }
}
