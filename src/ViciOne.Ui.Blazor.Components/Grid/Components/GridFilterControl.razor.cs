using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Localization.Resources;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;
using SearchBoxComponent = ViciOne.Ui.Blazor.Components.SearchBox.SearchBox;

namespace ViciOne.Ui.Blazor.Components.Grid.Components;

/// <summary>
/// Component for rendering a filter control in render fragment <see cref="Grid{TGridItem}.Filter"/>
/// </summary>
public sealed partial class GridFilterControl : ComponentBase
{
    private readonly string _iconCssClass =
        MonochromeIconName.FilterLight.GetCssClasses(SearchBoxComponent.IconSize).ToSpaceSeparated();

    private string? _placeholder;

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
    /// Raised when <see cref="Text" /> has changed
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

    /// <inheritdoc/>
    protected override void OnParametersSet()
        => _placeholder = Placeholder ?? TechnicalTerms.Filter;
}
