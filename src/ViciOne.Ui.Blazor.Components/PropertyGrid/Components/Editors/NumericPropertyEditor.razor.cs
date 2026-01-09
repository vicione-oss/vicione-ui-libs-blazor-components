using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components.Editors;

/// <summary>
/// Property editor for numeric values
/// </summary>
public sealed partial class NumericPropertyEditor<TPropertyValue, TInterval, TLimit>
    : PropertyEditorBase<TPropertyValue>, INumericPropertyEditor<TInterval, TLimit>, IHasAllowNull, IHasUpdateKey
{
    /// <inheritdoc />
    [Parameter] public required TInterval Interval { get; set; }

    /// <inheritdoc />
    [Parameter] public required bool IsRastered { get; set; }

    /// <inheritdoc />
    [Parameter, EditorRequired] public required TLimit Minimum { get; set; }

    /// <inheritdoc />
    [Parameter, EditorRequired] public required TLimit Maximum { get; set; }

    /// <inheritdoc />
    [Parameter] public bool AllowNull { get; set; }

    /// <inheritdoc />
    [Parameter] public object? UpdateKey { get; set; }
}
