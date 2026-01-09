namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models;

/// <inheritdoc cref="ISelectableValue{TPropertyValue}"/>
public sealed class SelectableValue<TPropertyValue> : ISelectableValue<TPropertyValue>
{
    /// <inheritdoc/>
    public required TPropertyValue Value { get; init; }

    /// <inheritdoc/>
    public required string Text { get; init; }
}
