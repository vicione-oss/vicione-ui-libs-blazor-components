using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components;

/// <summary>
/// Renders a group of properties.
/// </summary>
public sealed partial class PropertyGroup : ComponentBase
{
    private bool _expanded;
    private bool _isDescriptionAvailable;
    private bool _renderedOnce;

    /// <summary>
    /// Optional description displayed below <see cref="Header"/>.
    /// </summary>
    [Parameter] public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Text displayed in the header section.
    /// </summary>
    [Parameter] public string Header { get; set; } = string.Empty;

    /// <summary>
    /// <see langword="true" /> when the group should be considered a sub-group, otherwise <see langword="false" />.
    /// </summary>
    [Parameter] public bool IsSubGroup { get; set; }

    /// <summary>
    /// Items associated with the group, whereas a <see cref="PropertyEntry{TPropertyValue}"/> will be rendered for each item.
    /// </summary>
    [Parameter, EditorRequired] public required IEnumerable<IPropertyGridItem> Items { get; set; }

    /// <summary>
    /// <see langword="true" /> when the header should be visible, otherwise <see langword="false" />.
    /// </summary>
    [Parameter] public bool ShowHeader { get; set; } = true;

    /// <summary>
    /// Indentation multiplier
    /// </summary>
    [Parameter] public int SubGroupLayer { get; set; }

    /// <inheritdoc cref="IPropertyGridState.KeepMessages"/>
    [Parameter] public bool KeepMessages { get; set; }

    /// <inheritdoc/>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            _renderedOnce = true;
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        _isDescriptionAvailable = !string.IsNullOrWhiteSpace(Description);
        if (!_renderedOnce && !IsSubGroup)
            _expanded = true;
    }

    private void OnPropertyGroupContainerClick()
        => _expanded ^= true;
}
