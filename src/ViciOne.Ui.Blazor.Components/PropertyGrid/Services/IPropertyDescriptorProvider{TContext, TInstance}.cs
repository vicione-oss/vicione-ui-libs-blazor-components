using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

/// <summary>
/// Provider for property descriptors targeting types of <typeparamref name="TInstance"/> in scope of <typeparamref name="TContext"/>
/// </summary>
public interface IPropertyDescriptorProvider<TContext, TInstance> : IPropertyDescriptorProvider<TContext>
{
#pragma warning disable CA1033 // Interface methods should be callable by child types
    /// <inheritdoc/>
    Type IPropertyDescriptorProvider.GetTargetInstanceType() => typeof(TInstance);
#pragma warning restore CA1033 // Interface methods should be callable by child types

    /// <summary>
    /// Retrieves the property descriptors in scope of <typeparamref name="TContext"/>.
    /// </summary>
    IEnumerable<IPropertyDescriptor<TInstance>> GetPropertyDescriptors(TContext context);
}
