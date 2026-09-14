using ViciOne.Ui.Blazor.Components.Draggable.Components;

namespace ViciOne.Ui.Blazor.Components.Draggable.Services;

/// <summary>
/// Determines whether a drop on the specified <typeparamref name="TTarget"/> is allowed.
/// </summary>
public interface IDropPolicy<TTarget>
{
    /// <summary>
    /// Returns <see langword="true"/> when dropping <paramref name="draggable"/> on <paramref name="target"/> is allowed.
    /// </summary>
    bool Accepts(IDraggable draggable, TTarget target);
}
