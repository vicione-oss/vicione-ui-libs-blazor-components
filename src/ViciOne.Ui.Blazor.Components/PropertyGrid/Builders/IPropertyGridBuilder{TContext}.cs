using ViciOne.Ui.Blazor.Components.PropertyGrid.Components;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Builders;

/// <summary>
/// Builder for DI services required by the <see cref="PropertyGrid{TContext}"/> component.
/// </summary>
/// <typeparam name="TContext">Context associated with the property grid</typeparam>
public interface IPropertyGridBuilder<TContext>
{
    /// <summary>
    /// Adds <typeparamref name="TPropertyDescriptorProvider"/> as an implementation of
    /// <see cref="IPropertyDescriptorProvider{TContext}"/> to DI services.
    /// </summary>
    IPropertyGridBuilder<TContext> WithPropertyDescriptorProvider<TPropertyDescriptorProvider>()
        where TPropertyDescriptorProvider : class, IPropertyDescriptorProvider<TContext>;

    /// <summary>
    /// Adds <typeparamref name="TPropertyValueEqualityComparer"/> as an implementation of
    /// <see cref="IPropertyValueEqualityComparer"/> and <see cref="IPropertyValueEqualityComparer{TPropertyValue}"/>
    /// using service key <typeparamref name="TContext"/> to DI services.
    /// </summary>
    /// <remarks>
    /// When <typeparamref name="TPropertyValue"/> is a nullable reference type like <see langword="string?"/>
    /// then <typeparamref name="TPropertyValueEqualityComparer"/> MUST be implemented based on values of the
    /// non-nullable reference type because nullability is not propagated by the compiler.
    /// Luckily, the signature of <see cref="IEqualityComparer{TPropertyValue}.Equals"/>
    /// contains nullable parameters which also allows comparing values of nullable reference type.
    /// </remarks>
    IPropertyGridBuilder<TContext> WithPropertyValueEqualityComparer<TPropertyValue, TPropertyValueEqualityComparer>()
        where TPropertyValueEqualityComparer : class, IPropertyValueEqualityComparer<TPropertyValue>;
}
