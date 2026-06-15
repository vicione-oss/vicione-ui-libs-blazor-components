using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Moveable.Components;

namespace ViciOne.Ui.Blazor.Components.Popup.Components;

/// <summary>
/// A component that implements a layout with header and body for use in <see cref="Popup.ChildContent"/>.
/// </summary>
public sealed partial class PopupHeaderBodyLayout : IMoveHandle, IDisposable
{
    private ElementReference _headerElementReference;

    [CascadingParameter]
    private IMoveablePopup MoveablePopup { get; set; } = default!;

    /// <summary>
    /// The text rendered in the header section.
    /// </summary>
    [Parameter, EditorRequired]
    public string HeaderText { get; set; }

    /// <summary>
    /// Renders the content of the body section.
    /// </summary>
    [Parameter]
    public RenderFragment? Body { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
        => MoveablePopup?.RegisterMoveHandle(this);

    /// <inheritdoc/>
    public void Dispose()
        => MoveablePopup?.UnregisterMoveHandle(this);

    /// <inheritdoc/>
    public ElementReference GetElementReference()
        => _headerElementReference;
}
