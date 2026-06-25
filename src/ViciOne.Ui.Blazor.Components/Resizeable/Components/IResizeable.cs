using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Models;

namespace ViciOne.Ui.Blazor.Components.Resizeable.Components;

/// <summary>
/// A component that is resizeable.
/// </summary>
public interface IResizeable
{
    /// <summary>
    /// <see langword="true"/> when resizing is allowed, otherwise <see langword="false"/>.
    /// </summary>
    bool Resizeable { get; }

    /// <summary>
    /// Gets the element reference of the component for use in connection with JS interop.
    /// </summary>
    ElementReference GetElementReference();

    /// <summary>
    /// Gets the resize handles of the resizeable component.
    /// </summary>
    IReadOnlyCollection<IResizeHandle> GetResizeHandles();

    /// <summary>
    /// Registers a resize handle.
    /// </summary>
    void RegisterResizeHandle(IResizeHandle resizeHandle);

    /// <summary>
    /// Unregisters a resize handle.
    /// </summary>
    void UnregisterResizeHandle(IResizeHandle resizeHandle);

    /// <summary>
    /// Gets the container in which the resizeable component can resize.
    /// </summary>
    IResizeContainer GetResizeContainer();

    /// <summary>
    /// Gets the minimum width of the resizeable component.
    /// </summary>
    double GetMinimumWidth();

    /// <summary>
    /// Gets the minimum height of the resizeable component.
    /// </summary>
    double GetMinimumHeight();

    /// <summary>
    /// Updates the position and size of the resizeable component.
    /// </summary>
    Task UpdatePositionAndSizeAsync(DomRect domRect);
}
