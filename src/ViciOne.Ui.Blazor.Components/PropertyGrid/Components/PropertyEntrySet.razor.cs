using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components;

/// <summary>
/// Renders a set of properties.
/// </summary>
public partial class PropertyEntrySet : ComponentBase
{
    /// <summary>
    /// Indentation multiplier
    /// </summary>
    [Parameter]
    public int IndentationMultiplier { get; init; }

    /// <summary>
    /// <see langword="true" /> when the set of properties should be visible, otherwise <see langword="false" />.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <inheritdoc cref="IPropertyGridState.KeepMessages"/>
    [Parameter]
    public bool KeepMessages { get; set; }

    /// <summary>
    /// Items associated with the set, whereas a <see cref="PropertyEntry{TPropertyValue}"/> will be rendered for each item.
    /// </summary>
    [Parameter, EditorRequired]
    public required IEnumerable<IPropertyGridItem> Items { get; set; }

    /// <inheritdoc cref="IPropertyGridState.PropertyComparer"/>
    [Parameter]
    public IComparer<IPropertyGridItem>? PropertyComparer { get; set; }

    [Inject]
    private IAlphabeticalPropertyComparer DefaultPropertyComparer { get; set; } = default!;
}
