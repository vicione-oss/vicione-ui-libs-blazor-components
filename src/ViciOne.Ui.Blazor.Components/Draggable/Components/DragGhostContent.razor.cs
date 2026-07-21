using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Draggable.Components;

/// <summary>
/// Renders drag ghost content.
/// </summary>
/// <remarks>
/// The first Blazor render cycle renders the <see cref="ChildContent"/>, so the content is prepared for
/// <c>getContent</c> to read. Every later render must go through <see cref="RenderAsync"/>, which awaits the
/// flush and then raises <see cref="Rendered"/> so the hosting <see cref="DragGhostBase"/> can react (for
/// example, notify the JavaScript side to re-swap the drag ghost).
/// </remarks>
public sealed partial class DragGhostContent : ComponentBase
{
    private ElementReference _elementReference;
    private TaskCompletionSource? _pendingRender;
    private bool _shouldRender;

    /// <summary>
    /// The content rendered inside.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Raised after an explicit <see cref="RenderAsync"/> flush completes, once the freshly rendered content is
    /// live in the DOM. Not raised for the initial or ambient render cycle — only for on-demand refreshes.
    /// </summary>
    internal event Action? Rendered;

    internal ElementReference GetElementReference()
        => _elementReference;

    /// <inheritdoc/>
    protected override bool ShouldRender()
    {
        if (_shouldRender)
        {
            _shouldRender = false;

            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    protected override void OnAfterRender(bool firstRender)
    {
        if (_pendingRender is null)
            return;

        var pendingRender = _pendingRender;
        _pendingRender = null;

        pendingRender.TrySetResult();

        // The freshly rendered content is now live in the DOM. Let the host react (e.g. notify the JavaScript side to
        // re-swap the drag ghost) for renders that happened outside a lifecycle callback, such as a timer tick.
        Rendered?.Invoke();
    }

    /// <summary>
    /// Re-renders the current content into the host element and awaits the render flush, so the subtree exists
    /// client-side before it is cloned into the drag ghost. Superseding an in-flight render cancels it.
    /// </summary>
    internal Task RenderAsync()
        => InvokeAsync(async () =>
        {
            _pendingRender?.TrySetCanceled();

            var renderCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingRender = renderCompletionSource;

            _shouldRender = true;

            StateHasChanged();

            try
            {
                await renderCompletionSource.Task;
            }
            catch (OperationCanceledException)
            {
                // Superseded by a newer render — nothing to do.
            }
        });
}
