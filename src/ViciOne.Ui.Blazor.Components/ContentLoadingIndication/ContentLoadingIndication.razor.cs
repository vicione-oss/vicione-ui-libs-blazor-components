using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.ContentLoadingIndication.Enums;

namespace ViciOne.Ui.Blazor.Components.ContentLoadingIndication;

/// <summary>
/// Renders a loading indication for <see cref="ChildContent"/> when <see cref="Visible"/> is <see langword="true"/>,
/// otherwise simply renders <see cref="ChildContent"/>.
/// </summary>
public partial class ContentLoadingIndication : ComponentBase
{
    private bool _firstParameterSet = true;

    private bool _animateTransitions;
    private bool _visiblePassed;

    /// <summary>
    /// <see langword="true"/> when a <see cref="Kind"> loading indication</see> should be rendered, otherwise <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; }

    /// <summary>
    /// Kind of loading indication that should be rendered when <see cref="Visible"/> is <see langword="true"/>, otherwise parameter is ignored.
    /// </summary>
    [Parameter]
    public LoadingIndicationKind Kind { get; set; } = LoadingIndicationKind.GetDefaultValue();

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Defines the child components of this instance.
    /// </summary>
    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        _animateTransitions = (_visiblePassed != Visible) && !_firstParameterSet;

        _visiblePassed = Visible;

        _firstParameterSet = false;
    }
}
