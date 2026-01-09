using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

/// <summary>
/// Builds collections of <see cref="IPropertyGridItem"/> for a given set of instances and a context.
/// Implementations translate <see cref="IPropertyDescriptor"/> instances (retrieved via descriptor providers
/// associated with <typeparamref name="TContext"/>) into property-grid items consumable by the UI.
/// </summary>
/// <typeparam name="TContext">The context type that carries environment or metadata required to obtain descriptors
/// and influence item creation (for example services, options or descriptor providers).</typeparam>
public interface IPropertyGridItemCollectionBuilder<TContext>
{
    /// <summary>
    /// Builds a read-only collection of <see cref="IPropertyGridItem"/> for the provided <paramref name="instances"/>.
    /// </summary>
    /// <param name="instances">The target instances whose properties should be represented. Implementations should handle an empty sequence.</param>
    /// <param name="context">Contextual information used to resolve <see cref="IPropertyDescriptor"/> providers and influence building behavior.</param>
    /// <param name="messageStore">Message store to associate validation or informational messages produced during building. Implementations should record relevant messages in this store.</param>
    /// <returns>
    /// A read-only collection of property-grid items representing the properties discovered for the given instances.
    /// The collection may be empty when no descriptors are found or when the instances do not expose any properties.
    /// </returns>
    IReadOnlyCollection<IPropertyGridItem> Build(IEnumerable<object> instances, TContext context,
        IPropertyGridMessageStore messageStore);
}
