using Microsoft.AspNetCore.Components;
using Shared.Pages.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.ComboBox;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Messages;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace Shared.Pages.PropertyGrid.Components;

public sealed partial class PropertyGridPage : ComponentBase, IDisposable
{
    private const string OrderAscending = "A → Z";
    private const string OrderDescending = "Z → A";
    private const string OrderInsertion = "Insertion order";
    private const string OrderDefault = "Default (A → Z)";

    private ComboBoxItem<IComparer<IPropertyGridItem>?, string>[] _propertyOrderComboBoxItems = [];
    private ComboBoxItem<IComparer<string>?, string>[] _categoryOrderComboBoxItems = [];

    private readonly List<string> _changeLog = [];

    private interface IInstanceState<out T>
    {
        T Instance { get; }
        bool Selected { get; set; }
    }

    private class InstanceState<T> : IInstanceState<T>
        where T : class, new()
    {
        public T Instance { get; } = new();
        public bool Selected { get; set; }
    }

    private readonly InstanceState<ExampleFooInstance> _exampleFooInstanceState1 = new();
    private readonly InstanceState<ExampleFooInstance> _exampleFooInstanceState2 = new();
    private readonly InstanceState<ExampleBarInstance> _exampleBarInstanceState = new();

    private readonly List<IInstanceState<object>> _instanceStates = [];

    private bool _hasInfos;
    private bool _hasErrors;

    [Inject] private IPropertyGridController<ExamplePropertyGridContext> PropertyGridController { get; set; } = default!;
    [Inject] private IPropertyGridMessageStore<ExamplePropertyGridContext> PropertyGridMessageStore { get; set; } = default!;
    [Inject] private IPropertyGridEvents<ExamplePropertyGridContext> PropertGridEvents { get; set; } = default!;
    [Inject] private IPropertyGridState<ExamplePropertyGridContext> PropertyGridState { get; set; } = default!;

    [Inject] private IAlphabeticalPropertyComparer AlphabeticalPropertyComparer { get; set; } = default!;
    [Inject] private IReverseAlphabeticalPropertyComparer ReverseAlphabeticalPropertyComparer { get; set; } = default!;
    [Inject] private IInsertionOrderPropertyComparer InsertionOrderPropertyComparer { get; set; } = default!;

    [Inject] private IAlphabeticalCategoryComparer AlphabeticalCategoryComparer { get; set; } = default!;
    [Inject] private IReverseAlphabeticalCategoryComparer ReverseAlphabeticalCategoryComparer { get; set; } = default!;
    [Inject] private IInsertionOrderCategoryComparer InsertionOrderCategoryComparer { get; set; } = default!;

    protected override void OnInitialized()
    {
        _propertyOrderComboBoxItems =
        [
            new() { Value = AlphabeticalPropertyComparer, Text = OrderAscending },
            new() { Value = ReverseAlphabeticalPropertyComparer, Text = OrderDescending },
            new() { Value = InsertionOrderPropertyComparer, Text = OrderInsertion },
            new() { Value = null, Text = OrderDefault },
        ];

        _categoryOrderComboBoxItems =
        [
            new() { Value = AlphabeticalCategoryComparer, Text = OrderAscending },
            new() { Value = ReverseAlphabeticalCategoryComparer, Text = OrderDescending },
            new() { Value = InsertionOrderCategoryComparer, Text = OrderInsertion },
            new() { Value = null, Text = OrderDefault },
        ];

        _instanceStates.Add(_exampleFooInstanceState1);
        _instanceStates.Add(_exampleFooInstanceState2);
        _instanceStates.Add(_exampleBarInstanceState);

        PassSelectedInstancesToPropertyGridController();

        PropertyGridMessageStore.Changed += PropertyGridMessageStoreChanged;

        PropertGridEvents.PropertyChanged += PropertyGridPropertyChanged;
        PropertGridEvents.ContextMenuVisibilityChanged += PropertyGridContextMenuVisibilityChanged;

        PropertyGridState.GroupByCategory = true;
        PropertyGridState.PropertiesChanged += PropertyGridStatePropertiesChanged;
    }

    public void Dispose()
    {
        PropertyGridState.PropertiesChanged -= PropertyGridStatePropertiesChanged;

        PropertGridEvents.ContextMenuVisibilityChanged -= PropertyGridContextMenuVisibilityChanged;
        PropertGridEvents.PropertyChanged -= PropertyGridPropertyChanged;

        PropertyGridMessageStore.Changed -= PropertyGridMessageStoreChanged;
    }

    private void PropertyGridMessageStoreChanged(PropertyGridMessageStoreChangedEventArgs args)
    {
        var somethingChanged = false;

        var hasInfos = args.Sender.Contains<InfoMessage>();
        if (hasInfos != _hasInfos)
        {
            _hasInfos = hasInfos;

            somethingChanged = true;
        }

        var hasErrors = args.Sender.Contains<ErrorMessage>();
        if (hasErrors != _hasErrors)
        {
            _hasErrors = hasErrors;

            somethingChanged = true;
        }

        if (somethingChanged)
            InvokeAsync(StateHasChanged);
    }

    private void PassSelectedInstancesToPropertyGridController()
        => PropertyGridController.SetInstances(_instanceStates.Where(s => s.Selected).Select(s => s.Instance), new());

    private void InstanceSelectionChanged()
        => PassSelectedInstancesToPropertyGridController();

    private void PropertyGridContextMenuVisibilityChanged(PropertyGridContextMenuVisibilityChangedEventArgs args)
    {
        if (args.Visible)
            _changeLog.Add("Context menu visible");
        else
            _changeLog.Add("Context menu closed");

        InvokeAsync(StateHasChanged);
    }

    private void PropertyGridStatePropertiesChanged(PropertiesChangedEventArgs args)
    {
        if (args.PropertyNames.Contains(nameof(IPropertyGridState.Items)))
            InvokeAsync(StateHasChanged);
    }

    private void PropertyGridPropertyChanged(PropertyGridPropertyChangedEventArgs args)
    {
        _changeLog.Add($"{args.Item.DisplayName} changed");

        InvokeAsync(StateHasChanged);
    }

    private void SetNamePropertyToWaldoButtonClick()
    {
        foreach (var instance in _instanceStates.Where(s => s.Selected).Select(s => s.Instance).OfType<IHasName>())
        {
            instance.Name = "Waldo";

            PropertyGridController.UpdateProperty(nameof(instance.Name));
        }
    }

    private void FocusNamePropertyButtonClick()
        => PropertyGridController.FocusProperty(nameof(IHasName.Name));
}
