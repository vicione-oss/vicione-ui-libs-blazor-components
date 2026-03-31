using Microsoft.AspNetCore.Components;
using PopupComponent = ViciOne.Ui.Blazor.Components.Popup.Components.Popup;

namespace ViciOne.Ui.Blazor.Components.Dialog.Components;

/// <summary>
/// A component that displays dailog.
/// </summary>
public sealed partial class Dialog : ComponentBase
{
    private PopupComponent? _popup;

    /// <inheritdoc cref="PopupComponent.CssClass" />
    [Parameter] public string? CssClass { get; set; }

    /// <summary>
    /// The text rendered in the header section.
    /// </summary>
    [Parameter, EditorRequired] public string HeaderText { get; set; }

    /// <inheritdoc cref="PopupComponent.MinimumWidth"/>
    [Parameter] public string? MinimumWidth { get; set; } = "500px";

    /// <inheritdoc cref="PopupComponent.Width"/>
    [Parameter] public string? Width { get; set; }

    /// <inheritdoc cref="PopupComponent.Height"/>
    [Parameter] public string? Height { get; set; }

    /// <inheritdoc cref="PopupComponent.Visible"/>
    [Parameter] public bool Visible { get; set; }

    /// <summary>
    /// Raised when <see cref="Visible"/> has changed.
    /// </summary>
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    /// <summary>
    /// Renders additional content in the header between <see cref="HeaderText"/> and action buttons.
    /// </summary>
    [Parameter] public RenderFragment? HeaderSlot { get; set; }

    /// <summary>
    /// Renders the body of the dialog.
    /// </summary>
    /// <remarks>
    /// Use component <see cref="DialogBodyTextLayout"/> to render uniform layout or text content.
    /// </remarks>
    [Parameter] public RenderFragment? Body { get; set; }

    /// <summary>
    /// Renders the footer of the dialog.
    /// </summary>
    /// <remarks>
    /// Use component <see cref="DialogFooterButton"/> to render uniform footer buttons.
    /// </remarks>
    [Parameter] public RenderFragment? Footer { get; set; }

    /// <inheritdoc cref="PopupComponent.PreventBrowserContextMenu"/>
    [Parameter] public bool PreventBrowserContextMenu { get; set; }

    /// <inheritdoc cref="PopupComponent.OnShowing"/>
    [Parameter] public EventCallback OnShowing { get; set; }

    /// <inheritdoc cref="PopupComponent.OnClosing"/>
    [Parameter] public EventCallback OnClosing { get; set; }

    /// <summary>
    /// Shows the dialog.
    /// </summary>
    public async Task ShowAsync()
    {
        if (_popup is not null)
            await _popup.ShowAsync();
    }

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    public async Task CloseAsync()
    {
        if (_popup is not null)
            await _popup.CloseAsync();
    }

    private async Task PopupVisibleChangedAsync()
    {
        if (VisibleChanged.HasDelegate)
            await VisibleChanged.InvokeAsync(Visible);
    }
}
