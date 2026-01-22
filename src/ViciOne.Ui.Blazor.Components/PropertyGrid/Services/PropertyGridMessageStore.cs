using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Messages;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

internal class PropertyGridMessageStore<TContext> : IPropertyGridMessageStore<TContext>
{
    private readonly Dictionary<IPropertyGridItem, HashSet<IMessage>> _messageMap = [];
    private readonly HashSet<IPropertyGridItem> _itemsAdded = [];
    private readonly HashSet<IPropertyGridItem> _itemsRemoved = [];
    private readonly HashSet<IPropertyGridItem> _noItems = [];
    private readonly Lock _concurrentLock = new();
    private int _updateLock;

    public int Count => _messageMap.Count;

    public int UpdateLock => _updateLock;

    public ICollection<IPropertyGridItem> Keys => GetKeys();

    public event Action<PropertyGridMessageStoreChangedEventArgs>? Changed;

    private ICollection<IPropertyGridItem> GetKeys()
    {
        lock (_concurrentLock)
        {
            return [.. _messageMap.Keys];
        }
    }

    public bool Add(IPropertyGridItem propertyGridItem, IMessage message)
    {
        lock (_concurrentLock)
        {
            if (!_messageMap.TryGetValue(propertyGridItem, out var validationMessages))
            {
                validationMessages = [];

                _messageMap.Add(propertyGridItem, validationMessages);
            }

            if (validationMessages.Add(message))
            {
                if (UpdateLock == 0)
                {
                    Changed?.Invoke(new()
                    {
                        Sender = this,
                        Added = new HashSet<IPropertyGridItem> { propertyGridItem },
                        Removed = _noItems
                    });
                }
                else
                {
                    if (_itemsRemoved.Remove(propertyGridItem))
                    {
                        // item was removed before but now it is added again, consider it a no change
                    }
                    else
                    {
                        _itemsAdded.Add(propertyGridItem);
                    }
                }

                return true;
            }

            return false;
        }
    }

    public bool Clear()
    {
        lock (_concurrentLock)
        {
            if (_messageMap.Count == 0)
                return false;

            foreach (var propertyGridItem in _messageMap.Keys)
            {
                if (_itemsAdded.Remove(propertyGridItem))
                {
                    // item was added before but now it is removed again, consider it a no change
                }
                else
                {
                    _itemsRemoved.Add(propertyGridItem);
                }
            }

            _messageMap.Clear();

            if (UpdateLock == 0 && _itemsRemoved.Count > 0)
            {
                Changed?.Invoke(new()
                {
                    Sender = this,
                    Added = _noItems,
                    Removed = _itemsRemoved
                });

                _itemsRemoved.Clear();
            }

            return true;
        }
    }

    public bool Remove(IPropertyGridItem propertyGridItem)
    {
        lock (_concurrentLock)
        {
            var itemsRemovedCountBefore = _itemsRemoved.Count;

            if (_messageMap.Remove(propertyGridItem))
            {
                if (_itemsAdded.Remove(propertyGridItem))
                {
                    // item was added before but now it is removed again, consider it a no change
                }
                else
                {
                    _itemsRemoved.Add(propertyGridItem);
                }
            }

            var numberOfItemsRemoved = _itemsRemoved.Count - itemsRemovedCountBefore;

            if (numberOfItemsRemoved > 0)
            {
                if (UpdateLock == 0)
                {
                    Changed?.Invoke(new()
                    {
                        Sender = this,
                        Added = _noItems,
                        Removed = _itemsRemoved
                    });

                    _itemsRemoved.Clear();
                }
            }

            return numberOfItemsRemoved > 0;
        }
    }

    public IEnumerable<IMessage> Get(IPropertyGridItem propertyGridItem)
    {
        lock (_concurrentLock)
        {
            if (_messageMap.TryGetValue(propertyGridItem, out var messages))
                return messages;
            else
                return [];
        }
    }

    public void BeginUpdate()
    {
        lock (_concurrentLock)
        {
            _updateLock++;
        }
    }

    public void EndUpdate()
    {
        lock (_concurrentLock)
        {
            _updateLock--;

            if (_updateLock > 0)
                return;

            _updateLock = 0;

            if (_itemsAdded.Count > 0 || _itemsRemoved.Count > 0)
            {
                Changed?.Invoke(
                    new PropertyGridMessageStoreChangedEventArgs
                    {
                        Sender = this,
                        Added = _itemsAdded,
                        Removed = _itemsRemoved
                    });

                _itemsAdded.Clear();
                _itemsRemoved.Clear();
            }
        }
    }

    public bool Contains<TMessage>() where TMessage : IMessage
    {
        lock (_concurrentLock)
        {
            return _messageMap.Values.Any(messages => messages.OfType<TMessage>().Any());
        }
    }
}
