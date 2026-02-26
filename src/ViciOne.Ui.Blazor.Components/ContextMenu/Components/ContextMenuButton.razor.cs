using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Components;

/// <summary>
/// Component for rendering a context menu button in connection with <see cref="ContextMenuButtonRow"/>
/// </summary>
public sealed partial class ContextMenuButton : ContextMenuItemBase, IDisposable, IHasIcon
{
    private bool _isEnabled;

    [CascadingParameter]
    private ContextMenuButtonRow Parent { get; set; } = default!;

    /// <summary>
    /// True when the button should be enabled, otherwise false.
    /// </summary>
    /// <remarks>
    /// This property has lower priority than <see cref="ContextMenuButtonRow.Enabled"/>.
    /// </remarks>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Raised when the button has been clicked
    /// </summary>
    [Parameter]
    public EventCallback<ContextMenuButton> OnClick { get; set; }

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#attr-title">title</see> attribute
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public string? IconCssClass { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public Uri? IconUrl { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public string? IconData { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        Parent.AddButton(this);

        base.OnInitialized();
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (Parent == null)
        {
            throw new ArgumentNullException(nameof(Parent),
                $"{nameof(ContextMenuButton)} must exist within a {nameof(ContextMenuButtonRow)}");
        }

        _isEnabled = Parent.Enabled && Enabled;
    }

    /// <inheritdoc/>
    public void Dispose()
        => Parent?.RemoveButton(this);

    private async Task ButtonClickAsync()
    {
        if (_isEnabled)
        {
            await OnClick.InvokeAsync();

            await ParentContextMenu.HideAsync();
        }
    }
}
