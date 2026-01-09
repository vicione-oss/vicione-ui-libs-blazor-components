using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Interfaces;

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
    private readonly string _textBoxId = Guid.NewGuid().ToString();

    /// <summary>
    /// Text rendered as <see href="https://html.spec.whatwg.org/#attr-input-placeholder">placeholder</see> when <see cref="Text"/> is null or empty
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Text entered in the input element
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// Raised when the user enters something into the input element
    /// </summary>
    [Parameter]
    public EventCallback<string?> TextChanging { get; set; }

    /// <summary>
    /// Raised when the <see cref="Text" /> has changed.
    /// This is usually the case when either Enter was pressed or the input loses focus.
    /// </summary>
    [Parameter]
    public EventCallback<string?> TextChanged { get; set; }

    /// <summary>
    /// True when user interaction should be allowed, otherwise false
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Raised when the entered value was confirmed, that is either by pressing Enter or clicking the search icon
    /// </summary>
    [Parameter]
    public EventCallback ExecuteSearch { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public string? IconCssClass { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public Uri? IconUrl { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public string? IconData { get; set; }

    private async Task ExecuteSearchLabelClickAsync()
    {
        if (!Enabled)
            return;

        await InvokeExecuteSearchAsync();
    }

    private async Task TextBoxValueChangingAsync(string? value)
    {
        if (TextChanging.HasDelegate)
            await TextChanging.InvokeAsync(value);
    }

    private async Task TextBoxValueChangedAsync(string? value)
    {
        if (TextChanged.HasDelegate)
            await TextChanged.InvokeAsync(value);
    }

    private async Task TextBoxEnterPressedAsync(string? _)
        => await InvokeExecuteSearchAsync();

    private async Task InvokeExecuteSearchAsync()
    {
        if (ExecuteSearch.HasDelegate)
            await ExecuteSearch.InvokeAsync();
    }
}
