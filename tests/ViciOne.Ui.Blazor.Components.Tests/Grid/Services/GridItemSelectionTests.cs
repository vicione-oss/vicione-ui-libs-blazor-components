using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Grid.Extensions;
using ViciOne.Ui.Blazor.Components.Grid.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Grid.Services;

[Obsolete("Tests the obsolete Grid")]
public class GridItemSelectionTests
{
    [Fact]
    public void Should_be_resolvable()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var selection = serviceProvider.GetService<IGridItemSelection<Guid>>();

        // Assert
        selection.Should().NotBeNull();
    }

    [Fact]
    public void Should_be_resolvable_via_service_key()
    {
        // Arrange
        const string ServiceKey = "Foo";

        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>(ServiceKey);

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var selection = serviceProvider.GetKeyedService<IGridItemSelection<Guid>>(ServiceKey);

        // Assert
        selection.Should().NotBeNull();
    }

    [Fact]
    public void Assert_count()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        var selectionItem = Guid.NewGuid();

        // Act
        selection.Add(selectionItem);

        // Assert
        selection.Count.Should().Be(1);
    }

    [Fact]
    public void Assert_add_operation()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        var selectionItem = Guid.NewGuid();

        // Act
        selection.Add(selectionItem);

        // Assert
        selection.Should().BeEquivalentTo([selectionItem]);
    }

    [Fact]
    public void Should_not_add_same_item_twice()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        var selectionItem = Guid.NewGuid();
        selection.Add(selectionItem);

        // Act
        selection.Add(selectionItem);

        // Assert
        selection.Should().BeEquivalentTo([selectionItem]);
    }

    [Fact]
    public void Assert_add_range_operation()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        var selectionItem1 = Guid.NewGuid();
        var selectionItem2 = Guid.NewGuid();
        IEnumerable<Guid> selectionItems = [selectionItem1, selectionItem2];

        // Act
        selection.AddRange(selectionItems);

        // Assert
        selection.Should().BeEquivalentTo(selectionItems);
    }

    [Fact]
    public void Should_not_add_same_item_twice_via_add_range()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        var selectionItem1 = Guid.NewGuid();
        var selectionItem2 = Guid.NewGuid();
        var selectionItem3 = Guid.NewGuid();
        IEnumerable<Guid> selectionItems = [selectionItem1, selectionItem2, selectionItem3];

        selection.Add(selectionItem1);

        // Act
        selection.AddRange(selectionItems);

        // Assert
        selection.Should().BeEquivalentTo(selectionItems);
    }

    [Fact]
    public void Assert_clear_operation()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        var selectionItem = Guid.NewGuid();

        // Act
        selection.Clear();

        // Assert
        selection.Should().BeEmpty();
    }

    [Fact]
    public void Should_trigger_changed_event_on_add_operation()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();
        var selectionMonitor = selection.Monitor();

        var selectionItem = Guid.NewGuid();

        // Act
        selection.Add(selectionItem);

        // Assert
        selectionMonitor.Should().Raise(nameof(selection.Changed));
    }

    [Fact]
    public void Should_trigger_changed_event_on_clear_operation()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        var selectionItem = Guid.NewGuid();
        selection.Add(selectionItem);

        var selectionMonitor = selection.Monitor();

        // Act
        selection.Clear();

        // Assert
        selectionMonitor.Should().Raise(nameof(selection.Changed));
    }

    [Fact]
    public void Should_not_trigger_changed_event_after_begin_update()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        var selectionItem = Guid.NewGuid();
        selection.Add(selectionItem);

        var selectionMonitor = selection.Monitor();

        // Act
        selection.BeginUpdate();

        selection.Clear();
        selection.Add(selectionItem);

        // Assert
        selectionMonitor.Should().NotRaise(nameof(selection.Changed));
    }

    [Fact]
    public void Should_trigger_changed_event_on_end_update()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        var selectionItem1 = Guid.NewGuid();
        var selectionItem2 = Guid.NewGuid();

        var selectionChangedCounter = 0;
        selection.Changed += _ => selectionChangedCounter++;

        // Act
        selection.BeginUpdate();
        try
        {
            selection.Add(selectionItem1);
            selection.Add(selectionItem2);
        }
        finally
        {
            selection.EndUpdate();
        }

        // Assert
        selectionChangedCounter.Should().Be(1);
    }

    [Fact]
    public async Task Should_trigger_changed_event_on_outer_end_update_async()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        await using var serviceProvider = services.BuildServiceProvider();

        var selectionChangedCounter = 0;

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        selection.Changed += _ => ++selectionChangedCounter;

        static async Task RandomUpdateTaskAsync(IGridItemSelection<Guid> selection)
        {
            var delay = RandomNumberGenerator.GetInt32(100);
            await Task.Delay(delay);

            selection.BeginUpdate();
            try
            {
                var coinToss = RandomNumberGenerator.GetInt32(2);

                if (coinToss == 0)
                {
                    var firstSelectionItem = selection.FirstOrDefault();
                    if (firstSelectionItem != Guid.Empty)
                        selection.Remove(firstSelectionItem);
                    else
                        coinToss = 1;
                }

                if (coinToss == 1)
                {
                    var selectionItem = Guid.NewGuid();

                    selection.Add(selectionItem);
                }
            }
            finally
            {
                selection.EndUpdate();
            }
        }

        // Act
        selection.BeginUpdate();
        try
        {
            var updateTasks = new List<Task>();
            for (var i = 0; i < 1000; i++)
                updateTasks.Add(RandomUpdateTaskAsync(selection));

            await Task.WhenAll(updateTasks);

            // There is a chance that all tasks above add and remove the same item, hence no change would be raised.
            // So, we add an additional item with a nested BeginUpdate / EndUpdate here to ensure Change event is triggered
            selection.BeginUpdate();
            try
            {
                var selectionItem = Guid.NewGuid();

                selection.Add(selectionItem);
            }
            finally
            {
                selection.EndUpdate();
            }
        }
        finally
        {
            selection.EndUpdate();
        }

        // Assert
        selectionChangedCounter.Should().Be(1);
    }

    [Fact]
    public void Should_increase_update_lock_on_begin_update()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        // Act
        selection.BeginUpdate();

        // Assert
        selection.UpdateLock.Should().Be(1);
    }

    [Fact]
    public void Should_decrease_update_lock_on_end_update()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        // Act
        selection.BeginUpdate();
        selection.EndUpdate();

        // Assert
        selection.UpdateLock.Should().Be(0);
    }

    [Fact]
    public void Assert_changed_event_args()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        var selectionItem1 = Guid.NewGuid();
        var selectionItem2 = Guid.NewGuid();
        selection.Add(selectionItem1);
        selection.Add(selectionItem2);

        var selectionItem3 = Guid.NewGuid();
        var selectionItem4 = Guid.NewGuid();

        IGridItemSelection<Guid>? sender = null;
        IEnumerable<Guid>? itemsAdded = null;
        IEnumerable<Guid>? itemsRemoved = null;

        selection.Changed += eventArgs =>
        {
            sender = eventArgs.Sender;
            itemsAdded = [.. eventArgs.ItemsAdded];
            itemsRemoved = [.. eventArgs.ItemsRemoved];
        };

        // Act
        selection.BeginUpdate();
        try
        {
            selection.Remove(selectionItem1);
            selection.Add(selectionItem3);
            selection.Clear();
            selection.Add(selectionItem4);
        }
        finally
        {
            selection.EndUpdate();
        }

        // Assert
        sender.Should().NotBeNull().And.BeSameAs(selection);
        itemsAdded.Should().NotBeNull().And.BeEquivalentTo([selectionItem4]);
        itemsRemoved.Should().NotBeNull().And.BeEquivalentTo([selectionItem1, selectionItem2]);
    }

    [Fact]
    public void Should_not_trigger_changed_when_same_item_is_added_then_removed()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        var selectionItem = Guid.NewGuid();

        var selectionMonitor = selection.Monitor();

        // Act
        selection.BeginUpdate();
        try
        {
            selection.Add(selectionItem);
            selection.Remove(selectionItem);
        }
        finally
        {
            selection.EndUpdate();
        }

        // Assert
        selectionMonitor.Should().NotRaise(nameof(selection.Changed));
    }



    [Fact]
    public void Should_not_trigger_changed_when_same_item_is_removed_then_added()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddGridItemSelection<Guid>();

        using var serviceProvider = services.BuildServiceProvider();

        var selection = serviceProvider.GetRequiredService<IGridItemSelection<Guid>>();

        var selectionItem = Guid.NewGuid();
        selection.Add(selectionItem);

        var selectionMonitor = selection.Monitor();

        // Act
        selection.BeginUpdate();
        try
        {
            selection.Remove(selectionItem);
            selection.Add(selectionItem);
        }
        finally
        {
            selection.EndUpdate();
        }

        // Assert
        selectionMonitor.Should().NotRaise(nameof(selection.Changed));
    }
}
