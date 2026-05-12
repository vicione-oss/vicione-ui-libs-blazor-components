using ViciOne.Ui.Blazor.Components.Interfaces;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Messages;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

/// <summary>
/// Store for messages displayed in a property grid
/// </summary>
public interface IPropertyGridMessageStore : IHasUpdateLock
{
    /// <summary>
    /// Number of messages included in the store
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets a snapshot containing all the keys in the message store.
    /// </summary>
    ICollection<IPropertyGridItem> Keys { get; }

    /// <summary>
    /// Raised when the store has changed
    /// </summary>
    event Action<PropertyGridMessageStoreChangedEventArgs>? Changed;

    /// <summary>
    /// Determines whether the store contains any message of the specified type.
    /// </summary>
    /// <typeparam name="TMessage">The message type to look for.</typeparam>
    /// <returns>
    /// <see langword="true"/> when any <typeparamref name="TMessage"/> is contained in the store, otherwise <see langword="false"/>.
    /// </returns>
    bool Contains<TMessage>() where TMessage : IMessage;

    /// <summary>
    /// Clears all messages from the store
    /// </summary>
    bool Clear();

    /// <summary>
    /// Adds the given <paramref name="message"/> to the store for the given <paramref name="propertyGridItem"/>
    /// </summary>
    bool Add(IPropertyGridItem propertyGridItem, IMessage message);

    /// <summary>
    /// Removes all messages from the store for the given <paramref name="propertyGridItem"/>
    /// </summary>
    bool Remove(IPropertyGridItem propertyGridItem);

    /// <summary>
    /// Returns all messages for the given <paramref name="propertyGridItem"/>
    /// </summary>
    IEnumerable<IMessage> Get(IPropertyGridItem propertyGridItem);
}
