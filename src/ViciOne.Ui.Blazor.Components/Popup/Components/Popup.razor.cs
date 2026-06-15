using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Moveable.Components;
using ViciOne.Ui.Blazor.Components.Moveable.Services;
using ViciOne.Ui.Blazor.Components.Popup.Services;

namespace ViciOne.Ui.Blazor.Components.Popup.Components;

/// <summary>
/// A component that displays a popup.
/// </summary>
public sealed partial class Popup : ComponentBase, IPopup, IMoveable, IMoveHandle, IMoveContainer, IMoveablePopup, IAsyncDisposable
{
    private ElementReference _modalRootElementReference;
    private ElementReference _modalDialogElementReference;
    private IMoveHandle? _moveHandle;

    private bool _moveable;
    private Task? _moveInteractionAttachTask;
    private double? _x;
    private double? _y;
    private bool _requestRemoveMoveInteractionAfterRender;
    private bool _visible;
    private bool _showing;
    private bool _closing;
    private readonly SemaphoreSlim _showCloseSemaphore = new(1);
    private bool _disposed;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    /// <summary>
    /// Text rendered into the <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// of the dialog HTML element.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Specifies whether the component is visible.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the component should be visible, otherwise <see langword="false"/>.
    /// </value>
    [Parameter] public bool Visible { get; set; }

    /// <summary>
    /// Raised after render when <see cref="Visible"/> has changed when calling <see cref="ShowAsync()"/> or <see cref="CloseAsync()"/>.
    /// </summary>
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    /// <summary>
    /// The minimum width of the component in CSS units.
    /// </summary>
    [Parameter] public string? MinimumWidth { get; set; }

    /// <summary>
    /// Optional width of the component in CSS units.
    /// </summary>
    /// <remarks>
    /// If not set, the component adjusts its width to fit its content.
    /// </remarks>
    [Parameter] public string? Width { get; set; }

    /// <summary>
    /// Optional height of the component in CSS units.
    /// </summary>
    /// <remarks>
    /// If not set, the component adjusts its height to fit its content.
    /// </remarks>
    [Parameter] public string? Height { get; set; }

    /// <summary>
    /// Specifies whether a backdrop is shown behind the popup.
    /// </summary>
    /// <remarks>
    /// A backdrop is a semi-transparent overlay shown behind the popup that covers the entire viewport.
    /// </remarks>
    /// <value>
    /// <see langword="true"/> if the backdrop should be shown, otherwise <see langword="false"/>.
    /// </value>
    [Parameter] public bool ShowBackdrop { get; set; } = true;

    /// <summary>
    /// Controls the position of the popup in the
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/z-index">z-order</see>
    /// of elements.
    /// </summary>
    [Parameter] public int? ZIndex { get; set; }

    /// <summary>
    /// Renders the content of the popup.
    /// </summary>
    /// <remarks>
    /// It is recommended to use provided layout components like <see cref="PopupHeaderBodyLayout"/>
    /// for implementing content with a consistent layout across individual popups.
    /// </remarks>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <inheritdoc/>
    [Parameter] public bool Moveable { get; set; }

    /// <summary>
    /// <see langword="true"/> if the browser's context menu should not be displayed when the user requests it,
    /// otherwise <see langword="false"/>.
    /// </summary>
    [Parameter] public bool PreventBrowserContextMenu { get; set; }

    /// <summary>
    /// Raised when the component is going to be shown.
    /// </summary>
    [Parameter] public EventCallback OnShowing { get; set; }

    /// <summary>
    /// Raised when the component is going to be closed.
    /// </summary>
    [Parameter] public EventCallback OnClosing { get; set; }

    [Inject] private IPopupRegistry PopupRegistry { get; set; } = default!;
    [Inject] private IMoveInteraction MoveInteraction { get; set; } = default!;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        PopupRegistry.Add(this);
    }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        if (Moveable != _moveable)
        {
            _moveable = Moveable;

            if (!_moveable)
                _requestRemoveMoveInteractionAfterRender = true;
        }

        if (Visible != _visible && !_showing && !_closing)
        {
            _visible = Visible;

            _requestRemoveMoveInteractionAfterRender = true;

            if (_visible)
            {
                if (OnShowing.HasDelegate)
                    await OnShowing.InvokeAsync();
            }
            else
            {
                if (OnClosing.HasDelegate)
                    await OnClosing.InvokeAsync();
            }
        }
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (Interlocked.CompareExchange(ref _requestRemoveMoveInteractionAfterRender, false, true))
            await RemoveMoveInteractionAsync();

        if (_visible && _moveable && _moveInteractionAttachTask is null)
        {
            var isMoveHandleElementReferenceDefined = !string.IsNullOrEmpty(GetMoveHandle().GetElementReference().Id);

            if (isMoveHandleElementReferenceDefined)
            {
                _moveInteractionAttachTask = MoveInteraction.AttachAsync(this);

                await _moveInteractionAttachTask;
            }
        }

        if (Interlocked.CompareExchange(ref _showing, false, true))
        {
            if (VisibleChanged.HasDelegate)
                await VisibleChanged.InvokeAsync(_visible);
        }

        if (Interlocked.CompareExchange(ref _closing, false, true))
        {
            if (VisibleChanged.HasDelegate)
                await VisibleChanged.InvokeAsync(_visible);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposed, true, false))
            return;

        await RemoveMoveInteractionAsync();

        PopupRegistry.Remove(this);

        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();

        _showCloseSemaphore.Release();
        _showCloseSemaphore.Dispose();
    }

    /// <summary>
    /// Shows the popup.
    /// </summary>
    public async Task ShowAsync()
    {
        if (_disposed)
            return;

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            await _showCloseSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (_visible)
                    return;

                _visible = true;
                _showing = true;

                if (OnShowing.HasDelegate)
                    await OnShowing.InvokeAsync();

                await InvokeAsync(StateHasChanged);
            }
            finally
            {
                _showCloseSemaphore.Release();
            }
        }
        catch (OperationCanceledException)
        {
            // Nothing to do here, return gracefully
        }
        catch (ObjectDisposedException)
        {
            // Semaphore or CancellationTokenSource already disposed, nothing we can do, return gracefully
        }
    }

    /// <inheritdoc/>
    public async Task CloseAsync()
    {
        if (_disposed)
            return;

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            await _showCloseSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (!_visible)
                    return;

                _visible = false;
                _closing = true;
                _requestRemoveMoveInteractionAfterRender = true;

                if (OnClosing.HasDelegate)
                    await OnClosing.InvokeAsync();

                await InvokeAsync(StateHasChanged);
            }
            finally
            {
                _showCloseSemaphore.Release();
            }
        }
        catch (OperationCanceledException)
        {
            // Nothing to do here, return gracefully
        }
        catch (ObjectDisposedException)
        {
            // Semaphore or CancellationTokenSource already disposed, nothing we can do, return gracefully
        }
    }

    /// <inheritdoc/>
    ElementReference IMoveable.GetElementReference() => _modalDialogElementReference;

    /// <inheritdoc/>
    ElementReference IMoveHandle.GetElementReference() => _modalDialogElementReference;

    /// <inheritdoc/>
    ElementReference IMoveContainer.GetElementReference() => _modalRootElementReference;

    /// <inheritdoc/>
    public IMoveHandle GetMoveHandle() => _moveHandle ?? this;

    /// <inheritdoc/>
    public IMoveContainer GetMoveContainer() => this;

    /// <inheritdoc/>
    public async Task UpdatePositionAsync(double x, double y)
    {
        _x = x;
        _y = y;

        await InvokeAsync(StateHasChanged);
    }

    private async Task RemoveMoveInteractionAsync()
    {
        if (_moveInteractionAttachTask?.IsCompletedSuccessfully == true)
        {
            await MoveInteraction.RemoveAsync(this);

            _moveInteractionAttachTask = null;
        }
    }

    /// <inheritdoc/>
    public void RegisterMoveHandle(IMoveHandle moveHandle)
    {
        if (moveHandle != _moveHandle)
        {
            _moveHandle = moveHandle;

            _requestRemoveMoveInteractionAfterRender = true;
            InvokeAsync(StateHasChanged);
        }
    }

    /// <inheritdoc/>
    public void UnregisterMoveHandle(IMoveHandle moveHandle)
    {
        if (moveHandle == _moveHandle)
        {
            _moveHandle = null;

            _requestRemoveMoveInteractionAfterRender = true;
            InvokeAsync(StateHasChanged);
        }
    }
}
