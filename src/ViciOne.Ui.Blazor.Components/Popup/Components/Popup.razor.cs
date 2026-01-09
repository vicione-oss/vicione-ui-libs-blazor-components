using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Moveable.Interfaces;
using ViciOne.Ui.Blazor.Components.Moveable.Services;
using ViciOne.Ui.Blazor.Components.Popup.Services;

namespace ViciOne.Ui.Blazor.Components.Popup.Components;

/// <summary>
/// A component that displays a popup with backdrop.
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

    /// <summary>
    /// Text rendered into the <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// of the dialog HTML element.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Specifies whether the popup is visible.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the popup should be visible, otherwise <see langword="false"/>.
    /// </value>
    [Parameter] public bool Visible { get; set; }

    /// <summary>
    /// Raised when <see cref="Visible"/> has changed.
    /// </summary>
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    /// <summary>
    /// Optional width of the popup in CSS units.
    /// </summary>
    /// <remarks>
    /// If not set, the popup adjusts its width to fit its content.
    /// </remarks>
    [Parameter] public string? Width { get; set; }

    /// <summary>
    /// Optional height of the popup in CSS units.
    /// </summary>
    /// <remarks>
    /// If not set, the popup adjusts its height to fit its content.
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
    /// It is recommend to use provided layout components like <see cref="PopupHeaderBodyLayout"/>
    /// for implementing content with a consistent layout accross individual popups.
    /// </remarks>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <inheritdoc/>
    [Parameter] public bool Moveable { get; set; }

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
                await RemoveMoveInteractionAsync();
        }
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (Interlocked.CompareExchange(ref _requestRemoveMoveInteractionAfterRender, false, true))
            await RemoveMoveInteractionAsync();

        if (Visible && _moveable && _moveInteractionAttachTask is null)
        {
            var isMoveHandleElementReferenceDefined = !string.IsNullOrEmpty(GetMoveHandle().GetElementReference().Id);

            if (isMoveHandleElementReferenceDefined)
            {
                _moveInteractionAttachTask = MoveInteraction.AttachAsync(this);

                await _moveInteractionAttachTask;
            }
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await RemoveMoveInteractionAsync();

        PopupRegistry.Remove(this);

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async Task HideAsync()
    {
        if (!Visible)
            return;

        await RemoveMoveInteractionAsync();

        Visible = false;

        if (VisibleChanged.HasDelegate)
            await VisibleChanged.InvokeAsync(false);
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
    public void UpdatePosition(double x, double y)
    {
        _x = x;
        _y = y;

        InvokeAsync(StateHasChanged);
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
