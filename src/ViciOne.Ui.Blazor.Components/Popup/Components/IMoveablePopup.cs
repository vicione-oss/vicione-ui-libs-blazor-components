using ViciOne.Ui.Blazor.Components.Moveable.Components;

namespace ViciOne.Ui.Blazor.Components.Popup.Components;

/// <summary>
/// A component that implements a moveable popup.
/// </summary>
/// <remarks>
/// A moveable popup allows the user to move the popup via a move interaction.
///
/// <para>
/// A move interaction is started by left-click and holding the move handle registered with the popup.
/// </para>
///
/// <para>
/// If no move handle is registered, then any part of the popup can be used to start a move interaction.
/// </para>
/// </remarks>
public interface IMoveablePopup
{
    /// <summary>
    /// Registers a move handle for the popup.
    /// </summary>
    void RegisterMoveHandle(IMoveHandle moveHandle);

    /// <summary>
    /// Unregisters a move handle from the popup.
    /// </summary>
    void UnregisterMoveHandle(IMoveHandle moveHandle);
}
