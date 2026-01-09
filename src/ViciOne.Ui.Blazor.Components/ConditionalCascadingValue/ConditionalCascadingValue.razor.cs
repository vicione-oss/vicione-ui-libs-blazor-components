using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.ConditionalCascadingValue;

/// <summary>
/// A component that provides a cascading value to all descendant components
/// when <see cref="ConditionalCascadingValue{TValue}.Condition"/> is true.
/// </summary>
public sealed partial class ConditionalCascadingValue<TValue> : ComponentBase
{
    /// <inheritdoc cref="CascadingValue{TValue}.Value"/>
    [Parameter, EditorRequired]
    public required TValue Value { get; set; }

    /// <inheritdoc cref="CascadingValue{TValue}.Name"/>
    [Parameter]
    public string? Name { get; set; }

    /// <inheritdoc cref="CascadingValue{TValue}.IsFixed"/>
    [Parameter]
    public bool IsFixed { get; set; }

    /// <summary>
    /// True when the component should provide <see cref="Value"/> as cascading value to all descendant components, otherwise false.
    /// </summary>
    [Parameter, EditorRequired]
    public required bool Condition { get; set; }

    /// <inheritdoc cref="CascadingValue{TValue}.ChildContent"/>
    [Parameter, EditorRequired]
    public required RenderFragment ChildContent { get; set; }
}
