using System.Runtime.CompilerServices;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

internal sealed class PropertyGridState<TContext> : IPropertyGridState<TContext>
{
    private readonly Lock _concurrentLock = new();
    private readonly HashSet<string> _changedProperties = [];
    private int _updateLock;

    private IReadOnlyCollection<IPropertyGridItem>? _items;
    private IReadOnlyDictionary<IPropertyGridItem, HashSet<IPropertyGridItem>>? _itemDependentsMap;
    private bool _groupByCategory;
    private bool _keepMessages;

    public int UpdateLock => _updateLock;

    public IReadOnlyCollection<IPropertyGridItem> Items
    {
        get => _items ??= [];
        set
        {
            if (value != _items)
            {
                _items = value;

                OnPropertyChanged();
            }
        }
    }

    public IReadOnlyDictionary<IPropertyGridItem, HashSet<IPropertyGridItem>> ItemDependentsMap
    {
        get => _itemDependentsMap ??= new Dictionary<IPropertyGridItem, HashSet<IPropertyGridItem>>();
        set
        {
            if (value != _itemDependentsMap)
            {
                _itemDependentsMap = value;

                OnPropertyChanged();
            }
        }
    }

    public bool GroupByCategory
    {
        get => _groupByCategory;
        set
        {
            if (value != _groupByCategory)
            {
                _groupByCategory = value;

                OnPropertyChanged();
            }
        }
    }

    public bool KeepMessages
    {
        get => _keepMessages;
        set
        {
            if (value != _keepMessages)
            {
                _keepMessages = value;

                OnPropertyChanged();
            }
        }
    }

    public event Action<PropertiesChangedEventArgs>? PropertiesChanged;

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

            if (_updateLock <= 0)
            {
                _updateLock = 0;

                if (_changedProperties.Count == 0)
                    return;

                PropertiesChanged?.Invoke(new PropertiesChangedEventArgs(this, _changedProperties));

                _changedProperties.Clear();
            }
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (propertyName is null)
            return;

        if (_updateLock == 0)
            PropertiesChanged?.Invoke(new PropertiesChangedEventArgs(this, new HashSet<string> { propertyName }));
        else
            _changedProperties.Add(propertyName);
    }
}
