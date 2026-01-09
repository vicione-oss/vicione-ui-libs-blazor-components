using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.ComboBox;
using ViciOne.Ui.Blazor.Components.Interfaces;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components.Editors;

/// <summary>
/// Property editor that renders a combo-box which allows selection of predefined values
/// </summary>
public sealed partial class SelectionPropertyEditor<TPropertyValue>
    : PropertyEditorBase<TPropertyValue>, IHasUpdateKey
{
    /// <inheritdoc cref="ComboBox{TItem, TValue}.Items"/>
    [Parameter, EditorRequired]
    public required IEnumerable<ISelectableValue<TPropertyValue>> SelectableValues { get; set; }

    /// <inheritdoc cref="ComboBox{TItem, TValue}.NoOptionSelected"/>
    [Parameter]
    public bool? NoOptionSelected { get; set; }

    /// <inheritdoc />
    [Parameter]
    public object? UpdateKey { get; set; }
}
