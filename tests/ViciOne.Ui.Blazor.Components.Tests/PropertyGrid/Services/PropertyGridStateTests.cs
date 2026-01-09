using System.Security.Cryptography;
using AwesomeAssertions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.Tests.Enums;
using ViciOne.Ui.Blazor.Components.Tests.Interfaces;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Services;

public sealed class PropertyGridStateTests
{
    private readonly IHasChangeablePropertiesTests<PropertyGridState<object>> _tests = new();

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void Assert_changed_event_handling_when_group_by_category_is_set(bool initialValue, bool value,
        bool shouldTriggerChangedEvent)
            => _tests.AssertChangedEventHandlingWhenPropertyIsSet(state => state.GroupByCategory, initialValue,
                value, shouldTriggerChangedEvent);

    [Theory]
    [InlineData(AOrB.A, AOrB.A, false)]
    [InlineData(AOrB.A, AOrB.B, true)]
    [InlineData(AOrB.B, AOrB.A, true)]
    [InlineData(AOrB.B, AOrB.B, false)]
    public void Assert_changed_event_handling_when_items_is_set(AOrB initialValue, AOrB value, bool shouldTriggerChangedEvent)
    {
        var a = new List<IPropertyGridItem>();
        var b = new List<IPropertyGridItem>();

        _tests.AssertChangedEventHandlingWhenPropertyIsSet(state => state.Items,
            initialValue == AOrB.A ? a : b,
            value == AOrB.A ? a : b,
            shouldTriggerChangedEvent);
    }

    [Theory]
    [InlineData(AOrB.A, AOrB.A, false)]
    [InlineData(AOrB.A, AOrB.B, true)]
    [InlineData(AOrB.B, AOrB.A, true)]
    [InlineData(AOrB.B, AOrB.B, false)]
    public void Assert_changed_event_handling_when_items_dependents_map_is_set(AOrB initialValue, AOrB value,
        bool shouldTriggerChangedEvent)
    {
        var a = new Dictionary<IPropertyGridItem, HashSet<IPropertyGridItem>>();
        var b = new Dictionary<IPropertyGridItem, HashSet<IPropertyGridItem>>();

        _tests.AssertChangedEventHandlingWhenPropertyIsSet(state => state.ItemDependentsMap,
            initialValue == AOrB.A ? a : b,
            value == AOrB.A ? a : b,
            shouldTriggerChangedEvent);
    }

    [Fact]
    public void Should_not_trigger_changed_event_after_begin_update()
    {
        // Arrange
        var state = new PropertyGridState<object>();

        var propertiesChanged = false;

        state.PropertiesChanged += _ => propertiesChanged = true;

        // Act
        state.BeginUpdate();

        state.GroupByCategory = true;
        state.Items = [];

        // Assert
        propertiesChanged.Should().Be(false);
    }

    [Fact]
    public void Should_trigger_changed_event_on_end_update_with_correct_args()
    {
        // Arrange
        var state = new PropertyGridState<object>();

        var propertiesChangedRaiseCount = 0;
        var affectedPropertyNames = new List<string>();

        state.PropertiesChanged += args =>
        {
            propertiesChangedRaiseCount++;
            affectedPropertyNames.AddRange(args.PropertyNames);
        };

        // Act
        state.BeginUpdate();
        try
        {
            state.GroupByCategory = true;
            state.Items = [];
        }
        finally
        {
            state.EndUpdate();
        }

        // Assert
        propertiesChangedRaiseCount.Should().Be(1);
        affectedPropertyNames.Should().HaveCount(2);
    }

    [Fact]
    public async Task Should_trigger_changed_event_on_outer_end_update()
    {
        // Arrange
        var propertiesChangedRaiseCount = 0;

        var state = new PropertyGridState<object>();
        state.PropertiesChanged += _ => ++propertiesChangedRaiseCount;

        static async Task RandomUpdateTaskAsync(PropertyGridState<object> state)
        {
            var delay = RandomNumberGenerator.GetInt32(100);
            await Task.Delay(delay);

            state.BeginUpdate();
            try
            {
                var coinToss = RandomNumberGenerator.GetInt32(2);

                if (coinToss == 0)
                    state.GroupByCategory = !state.GroupByCategory;
                else
                    state.Items = [];
            }
            finally
            {
                state.EndUpdate();
            }
        }

        // Act
        state.BeginUpdate();
        try
        {
            var updateTasks = new List<Task>();
            for (var i = 0; i < 1000; i++)
                updateTasks.Add(RandomUpdateTaskAsync(state));

            await Task.WhenAll(updateTasks);
        }
        finally
        {
            state.EndUpdate();
        }

        // Assert
        propertiesChangedRaiseCount.Should().Be(1);
    }

    [Fact]
    public void Should_increase_update_lock_on_begin_update()
    {
        // Arrange
        var state = new PropertyGridState<object>();

        // Act
        state.BeginUpdate();

        // Assert
        state.UpdateLock.Should().Be(1);
    }

    [Fact]
    public void Should_decrease_update_lock_on_end_update()
    {
        // Arrange
        var state = new PropertyGridState<object>();

        // Act
        state.BeginUpdate();
        state.EndUpdate();

        // Assert
        state.UpdateLock.Should().Be(0);
    }
}
