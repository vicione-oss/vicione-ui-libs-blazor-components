using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components.Editors;

/// <summary>
/// Base class for property editor components
/// </summary>
public abstract class PropertyEditorBase<TPropertyValue> : ComponentBase
{
    /// <summary>
    /// The value of the property being edited.
    /// </summary>
    [Parameter, EditorRequired] public required TPropertyValue Value { get; set; }

    /// <summary>
    /// Callback invoked when the value changes.
    /// </summary>
    [Parameter] public EventCallback<TPropertyValue> ValueChanged { get; set; }

    /// <summary>
    /// Indicates whether the property editor is enabled.
    /// </summary>
    [Parameter] public bool Enabled { get; set; }

    /// <summary>
    /// Indicates whether the property editor is read-only.
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>
    /// Indicates whether the property editor is valid.
    /// </summary>
    [Parameter] public bool? Valid { get; set; }

    /// <summary>
    /// The expression representing the value being edited.
    /// </summary>
    protected Expression<Func<TPropertyValue>>? ValueExpression => () => Value;

    /// <summary>
    /// Reference to the inner editor component, if any.
    /// </summary>
    public object? InnerEditorComponentReference { get; protected set; }
}
