using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;

namespace ViciOne.Ui.Blazor.Components.PointerCapture.Services;

/// <summary>
/// Snap to grid behavior for pointer capture operations
/// </summary>
public interface ISnapToGridPointerCaptureBehavior : IPointerCaptureBehavior
{
    /// <summary>
    /// Sets the grid size to the given <paramref name="value"/>.
    /// </summary>
    Task SetGridSizeAsync(int value);
}
