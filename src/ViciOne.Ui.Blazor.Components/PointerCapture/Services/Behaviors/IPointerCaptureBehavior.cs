using Microsoft.JSInterop;

namespace ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;

/// <summary>
/// Behavior for pointer capture operations
/// </summary>
public interface IPointerCaptureBehavior
{
    /// <summary>
    /// Gets the reference of the JS object implementing this behavior.
    /// </summary>
    Task<IJSObjectReference?> GetJsObjectAsync();
}
