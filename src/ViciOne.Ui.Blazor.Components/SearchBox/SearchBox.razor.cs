using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Interfaces;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using TextBoxComponent = ViciOne.Ui.Blazor.Components.TextBox.TextBox;

namespace ViciOne.Ui.Blazor.Components.SearchBox;

/// <summary>
/// Component for rendering a search box
/// </summary>
/// <remarks>
/// The component implements <see cref="IHasIcon"/> to allow configuration of the search icon.
/// If no icon property is set then the component uses a fallback to the default search icon.
/// </remarks>
public sealed partial class SearchBox : ComponentBase, IHasIcon
{
    private TextBoxComponent? _textBox;
    private bool _isSomeTextEntered;
    private bool _animateTransitions;

    internal static MonochromeIconSize IconSize => MonochromeIconSize.SmallPlus2;

    /// <summary>
    /// Text rendered as <see href="https://html.spec.whatwg.org/#attr-input-placeholder">placeholder</see>
    /// when <see cref="Text"/> is <see langword="null"/> or empty.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Text entered in the input element.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// Raised when the user enters something into the input element or the input was cleared.
    /// </summary>
    [Parameter]
    public EventCallback<string?> TextChanging { get; set; }

    /// <summary>
    /// Raised when the input has changed.
    /// This is the case when either Enter was pressed, the input was cleared or the input loses focus.
    /// </summary>
    [Parameter]
    public EventCallback<string?> TextChanged { get; set; }

    /// <summary>
    /// Raised when Enter was pressed in the input element, after <see cref="TextChanged"/> has committed the value.
    /// </summary>
    [Parameter]
    public EventCallback<string?> EnterPressed { get; set; }

    /// <summary>
    /// <see langword="true"/> when user interaction should be allowed, otherwise <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

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
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        _isSomeTextEntered = !string.IsNullOrEmpty(Text);
    }

    private async Task TextBoxValueChangingAsync(string? value)
    {
        UpdateFlags(value);

        if (TextChanging.HasDelegate)
            await TextChanging.InvokeAsync(value);
    }

    private async Task TextBoxValueChangedAsync(string? value)
    {
        // We need to update flags here too as changing focus immediatelly after keypress does not trigger TextBoxValueChangingAsync
        UpdateFlags(value);

        Text = value;

        if (TextChanged.HasDelegate)
            await TextChanged.InvokeAsync(value);
    }

    private void UpdateFlags(string? value)
    {
        _isSomeTextEntered = !string.IsNullOrEmpty(value);
        _animateTransitions = true;
    }

    private async Task ClearButtonClickAsync()
    {
        Text = null;

        _isSomeTextEntered = false;

        if (TextChanging.HasDelegate)
            await TextChanging.InvokeAsync(null);

        if (TextChanged.HasDelegate)
            await TextChanged.InvokeAsync(null);

        if (_textBox is not null)
            await _textBox.FocusAsync();
    }

    private async Task TextBoxEnterPressedAsync()
    {
        if (EnterPressed.HasDelegate)
            await EnterPressed.InvokeAsync(Text);
    }
}
