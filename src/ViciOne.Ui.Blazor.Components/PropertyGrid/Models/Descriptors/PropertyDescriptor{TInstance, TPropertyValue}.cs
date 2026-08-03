using ViciOne.Ui.Blazor.Components.Helpers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Validators;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

/// <inheritdoc cref="IPropertyDescriptor{TInstance, TPropertyValue}"/>
public class PropertyDescriptor<TInstance, TPropertyValue> : IPropertyDescriptor<TInstance, TPropertyValue>
{
    private bool _canBeSetToNull;

    /// <inheritdoc/>
    public required string Name { get; set; }

    /// <inheritdoc/>
    /// <remarks>
    /// Defaults to <see langword="false"/> because nullability based on <typeparamref name="TPropertyValue"/>
    /// cannot be determined for reference types securely due to limitation of the C# compiler.
    ///
    /// <para>
    ///     It is expected that this property is set manually to <see langword="true"/> when
    ///     <typeparamref name="TPropertyValue"/> is a nullable type and null is a desired property value.
    /// </para>
    ///
    /// <para>
    ///     The setter will throw an <see cref="ArgumentException"/> if <see langword="true"/> is set and
    ///     <typeparamref name="TPropertyValue"/> is not nullable.
    /// </para>
    /// </remarks>
    public bool CanBeSetToNull
    {
        get => _canBeSetToNull;

        set
        {
            if (value && !GenericParameterHelper.IsNullable<TPropertyValue>())
            {
                throw new ArgumentException(
                    $"{nameof(CanBeSetToNull)} was set to true but {nameof(TPropertyValue)} is not nullable", nameof(value));
            }

            _canBeSetToNull = value;
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Defaults to <see cref="string.Empty"/> to avoid the need to set the property when
    /// <see cref="IPropertyGridState.GroupByCategory"/> is always <see langword="false"/>.
    /// </remarks>
    public string Category { get; set; } = string.Empty;

    /// <inheritdoc/>
    public IEnumerable<IPropertyValueValidator<TPropertyValue>>? ValueValidators { get; set; }

    /// <inheritdoc/>
    public Func<TInstance, TPropertyValue>? GetDefaultValue { get; set; }

    /// <inheritdoc/>
    public Action<TInstance, TPropertyValue>? SetValue { get; set; }

    /// <inheritdoc/>
    public Func<TInstance, bool>? Resettable { get; set; }

    /// <inheritdoc/>
    public Action<TInstance>? ResetValue { get; set; }

    /// <inheritdoc/>
    public string? Description { get; set; }

    /// <inheritdoc/>
    public required Func<TInstance, TPropertyValue> GetValue { get; set; }

    /// <inheritdoc/>
    public string? InformationTooltip { get; set; }

    /// <inheritdoc/>
    public Func<TInstance, bool>? Enabled { get; set; }

    /// <inheritdoc/>
    public Func<TInstance, bool>? ReadOnly { get; set; }

    /// <inheritdoc/>
    public string? DisplayName { get; set; }

    /// <inheritdoc/>
    public Type TargetType => typeof(TInstance);

    /// <inheritdoc/>
    public Type ValueType => typeof(TPropertyValue);

    /// <inheritdoc/>
    public Func<TInstance, bool>? ConsiderPredicate { get; set; }

    /// <inheritdoc/>
    public Func<TInstance, bool>? Visible { get; set; }

    /// <inheritdoc/>
    public IEnumerable<IPropertyDescriptor>? DependsOn { get; set; }

    /// <inheritdoc/>
    public Func<TInstance, TPropertyValue, bool>? HasValueDifferentFromDefaultValue { get; set; }
}
