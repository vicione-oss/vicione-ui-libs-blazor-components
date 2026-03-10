using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Resizing.Models;

namespace ViciOne.Ui.Blazor.Components.Resizing.Services;

/// <summary>
/// Monitors the dimensions of <see href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLElement">HTML elements</see>
/// using the <see href="https://developer.mozilla.org/en-US/docs/Web/API/ResizeObserver">ResizeObserver API</see>.
/// </summary>
public interface IResizeObserver
{
    /// <summary>
    /// Occurs when an observed element's dimensions change.
    /// Use this for synchronous UI updates or state changes that do not require awaited tasks.
    /// </summary>
    event Action<ElementSizeChangedEventArgs>? ElementSizeChanged;

    /// <summary>
    /// Occurs when an observed element's dimensions change.
    /// Use this when the resize event needs to trigger asynchronous logic, such as data fetching or complex animations.
    /// </summary>
    event Func<ElementSizeChangedEventArgs, Task>? ElementSizeChangedAsync;

    /// <summary>
    /// Starts tracking the specified HTML element.
    /// When the element resizes, the <see cref="ElementSizeChanged"/> or <see cref="ElementSizeChangedAsync"/> event will be fired.
    /// </summary>
    /// <param name="elementReference">The Blazor reference to the HTML element.</param>
    /// <param name="includeStyle">When set to <see langword="true"/>, <see cref="ElementSizeChangedEventArgs.Style"/> is filled in and passed with events.</param>
    Task ObserveAsync(ElementReference elementReference, bool includeStyle = false);

    /// <summary>
    /// Stops tracking the specified HTML element.
    /// </summary>
    /// <param name="elementReference">The Blazor reference to the HTML element currently being observed.</param>
    Task UnobserveAsync(ElementReference elementReference);
}
