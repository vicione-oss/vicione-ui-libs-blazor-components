using System.Collections.Concurrent;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.ContextMenu.Services;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

internal sealed class PropertyGridController<TContext>(IPropertyGridMessageStore<TContext> messageStore,
    IPropertyGridItemCollectionBuilder<TContext> itemCollectionBuilder,
    IPropertyGridEvents<TContext> events,
    IPropertyGridState<TContext> state,
    IServiceProvider serviceProvider)
        : IPropertyGridController<TContext>, IDisposable
{
    private const int TimerInterval = 20;
    private int _disposed;

    private IEnumerable<object>? _instances;
    private TContext? _context;
    private readonly System.Timers.Timer _instancesChangedTimer = new()
    {
        AutoReset = false,
        Interval = TimerInterval
    };

    private readonly record struct UpdatePropertyCall(string PropertyName);

    private readonly ConcurrentQueue<UpdatePropertyCall> _updatePropertyCalls = new();
    private readonly System.Timers.Timer _updatePropertyTimer = new()
    {
        AutoReset = false,
        Interval = TimerInterval
    };

    private IContextMenuRequest<PropertyEntryContextMenuContext>? _contextMenuRequest;

    public IPropertyGridState State => state;
    public IPropertyGridEvents Events => events;

    public event Action<PropertyGridControllerFocusPropertyRequestedEventArgs>? FocusPropertyRequested;
    public event Action<PropertyGridControllerUpdatePropertyRequestedEventArgs>? UpdatePropertyRequested;

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
        {
            _instancesChangedTimer.Elapsed -= InstancesChangedTimerElapsed;
            _instancesChangedTimer.Dispose();

            _updatePropertyTimer.Elapsed -= UpdatePropertyTimerElapsed;
            _updatePropertyTimer.Dispose();
        }
    }

    public void SetInstances(IEnumerable<object> instances, TContext context)
    {
        _instances = instances;
        _context = context;

        _instancesChangedTimer.Stop();

        _instancesChangedTimer.Elapsed -= InstancesChangedTimerElapsed;
        _instancesChangedTimer.Elapsed += InstancesChangedTimerElapsed;

        _instancesChangedTimer.Start();
    }

    private void InstancesChangedTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_disposed == 1)
            return;

        messageStore.Clear();

        if (_instances is not null && _context is not null)
        {
            state.BeginUpdate();
            try
            {
                state.Items = itemCollectionBuilder.Build(_instances, _context, messageStore);
                state.ItemDependentsMap = CreatePropertyGridItemDependentsMap(state.Items);
            }
            finally
            {
                state.EndUpdate();
            }
        }
    }

    public void FocusProperty(string name)
    {
        var args = new PropertyGridControllerFocusPropertyRequestedEventArgs { Name = name };

        FocusPropertyRequested?.Invoke(args);
    }

    void IPropertyGridController.UpdateDependents(IPropertyGridItem propertyGridItem)
    {
        var updateTargets = new HashSet<IPropertyGridItem>();

        AddDependentsAsUpdateTarget(propertyGridItem, updateTargets);

        var args = new PropertyGridControllerUpdatePropertyRequestedEventArgs { Targets = updateTargets };

        UpdatePropertyRequested?.Invoke(args);
    }

    public void UpdateProperty(string name)
        => EnqueueUpdateProperty(new UpdatePropertyCall(name));

    private void EnqueueUpdateProperty(UpdatePropertyCall updateCall)
    {
        if (_updatePropertyCalls.Contains(updateCall))
            return;

        _updatePropertyCalls.Enqueue(updateCall);

        _updatePropertyTimer.Stop();

        _updatePropertyTimer.Elapsed -= UpdatePropertyTimerElapsed;
        _updatePropertyTimer.Elapsed += UpdatePropertyTimerElapsed;

        _updatePropertyTimer.Start();
    }

    private void UpdatePropertyTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_disposed == 1)
            return;

        var items = state.Items;
        if (items.Count < 1)
            return;

        var itemMap = items.ToDictionary(i => i.Name);

        var updateTargets = new HashSet<IPropertyGridItem>();

        while (_updatePropertyCalls.TryDequeue(out var updatePropertyCall))
        {
            if (itemMap.TryGetValue(updatePropertyCall.PropertyName, out var propertyGridItem))
            {
                if (updateTargets.Add(propertyGridItem))
                    AddDependentsAsUpdateTarget(propertyGridItem, updateTargets);
            }
        }

        var args = new PropertyGridControllerUpdatePropertyRequestedEventArgs { Targets = updateTargets };

        UpdatePropertyRequested?.Invoke(args);
    }

    private void AddDependentsAsUpdateTarget(IPropertyGridItem propertyGridItem, HashSet<IPropertyGridItem> updateTargets)
    {
        if (state.ItemDependentsMap?.TryGetValue(propertyGridItem, out var dependents) == true)
        {
            foreach (var dependent in dependents)
                updateTargets.Add(dependent);
        }
    }

    private static void GetDependsOnRecursive(IPropertyGridItem rootPropertyGridItem, IPropertyGridItem currentPropertyGridItem,
        IDictionary<IPropertyDescriptor, IPropertyGridItem> propertyGridItemMap, HashSet<IPropertyGridItem> dependsOn)
    {
        foreach (var propertyDescriptor in currentPropertyGridItem.PropertyDescriptors)
        {
            if (propertyDescriptor.DependsOn is null)
                continue;

            foreach (var dependencyPropertyDescriptor in propertyDescriptor.DependsOn)
            {
                if (propertyGridItemMap.TryGetValue(dependencyPropertyDescriptor, out var dependencyPropertyGridItem))
                {
                    if (dependencyPropertyGridItem == rootPropertyGridItem)
                        continue;

                    if (dependsOn.Add(dependencyPropertyGridItem))
                        GetDependsOnRecursive(rootPropertyGridItem, dependencyPropertyGridItem, propertyGridItemMap, dependsOn);
                }
            }
        }
    }

    /// <summary>
    /// Creates a dictionary that maps each PropertyGridItem to its dependents whereas the term dependents refers
    /// to the reverse view of a dependency. Dependents are also handled recursively here.
    /// </summary>
    /// <remarks>
    /// Dependents are also handled recursively.
    /// So, if C depends on B and B depends on A then A has two dependents (B and C) and B has one dependent (C).
    /// </remarks>
    private static Dictionary<IPropertyGridItem, HashSet<IPropertyGridItem>> CreatePropertyGridItemDependentsMap(
        IReadOnlyCollection<IPropertyGridItem> items)
    {
        Dictionary<IPropertyDescriptor, IPropertyGridItem> propertyGridItemMap = [];

        // Determine which property descriptor is associated with which property grid item in a map
        foreach (var item in items)
        {
            foreach (var propertyDescriptor in item.PropertyDescriptors)
                propertyGridItemMap[propertyDescriptor] = item;
        }

        // DependsOn
        var dependsOnMap = new Dictionary<IPropertyGridItem, HashSet<IPropertyGridItem>>();

        foreach (var item in items)
        {
            var dependsOn = new HashSet<IPropertyGridItem>();

            GetDependsOnRecursive(rootPropertyGridItem: item, currentPropertyGridItem: item, propertyGridItemMap, dependsOn);

            if (dependsOn.Count > 0)
                dependsOnMap.Add(item, dependsOn);
        }

        // Dependents
        var result = new Dictionary<IPropertyGridItem, HashSet<IPropertyGridItem>>();

        foreach (var p in dependsOnMap)
        {
            var item = p.Key;
            var dependsOnRecursive = p.Value;

            foreach (var dependsOn in dependsOnRecursive)
            {
                if (!result.TryGetValue(dependsOn, out var dependents))
                {
                    dependents = [];

                    result.Add(dependsOn, dependents);
                }

                dependents.Add(item);
            }
        }

        return result;
    }

    public async Task ShowContextMenuAsync(PropertyEntryContextMenuContext context)
    {
        var contextMenuServiceKey = typeof(TContext);

        _contextMenuRequest ??= serviceProvider.GetRequiredKeyedService<IContextMenuRequest<PropertyEntryContextMenuContext>>(
            contextMenuServiceKey);

        await _contextMenuRequest.SendAsync(context);
    }
}
