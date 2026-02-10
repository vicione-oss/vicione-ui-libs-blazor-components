using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Moveable.Interfaces;

/// <summary>
/// A component that is moveable.
/// </summary>
public interface IMoveable
{
    /// <summary>
    /// <see langword="true"/> when moving is allowed, otherwise <see langword="false"/>.
    /// </summary>
    bool Moveable { get; }

    /// <summary>
    /// Gets the element reference of the component for use in connection with JS interop.
    /// </summary>
    ElementReference GetElementReference();

    /// <summary>
    /// Gets the move handle of the moveable component.
    /// </summary>
    IMoveHandle GetMoveHandle();

    /// <summary>
    /// Gets the container in which the moveable component can move.
    /// </summary>
    IMoveContainer GetMoveContainer();

    /// <summary>
    /// Updates the position of the moveable component.
    /// </summary>
    /// <param name="x">Horizontal position</param>
    /// <param name="y">Vertical position</param>
    Task UpdatePositionAsync(double x, double y);
}
