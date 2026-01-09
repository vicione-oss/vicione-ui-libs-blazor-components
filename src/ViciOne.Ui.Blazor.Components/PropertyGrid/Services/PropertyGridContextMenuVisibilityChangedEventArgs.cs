namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

/// <summary>
/// Arguments for the <see cref="IPropertyGridEvents.ContextMenuVisibilityChanged"/> event.
/// </summary>
public sealed class PropertyGridContextMenuVisibilityChangedEventArgs : EventArgs
{
    /// <summary>
    /// Instance that raised the associated <see cref="IPropertyGridEvents.ContextMenuVisibilityChanged"/> event.
    /// </summary>
    public required IPropertyGridEvents Sender { get; init; }

    /// <summary>
    /// <see langword="true"/> if the context menu is visible, otherwise <see langword="false"/>.
    /// </summary>
    public required bool Visible { get; init; }
}
