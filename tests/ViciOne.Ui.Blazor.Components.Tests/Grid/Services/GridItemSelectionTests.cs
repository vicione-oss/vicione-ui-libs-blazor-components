using System.Security.Cryptography;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Grid.Extensions;
using ViciOne.Ui.Blazor.Components.Grid.Services;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Grid.Services;

public class GridItemSelectionTests
{
    [Fact]
    public void ShouldBeResolvable()
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
    public void ShouldBeResolvableViaServiceKey()
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
    public void AssertCount()
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
    public void AssertAddOperation()
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
    public void ShouldNotAddSameItemTwice()
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
    public void AssertAddRangeOperation()
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
    public void ShouldNotAddSameItemTwiceViaAddRange()
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
    public void AssertClearOperation()
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
    public void ShouldTriggerChangedEventOnAddOperation()
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
    public void ShouldTriggerChangedEventOnClearOperation()
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
    public void ShouldNotTriggerChangedEventAfterBeginUpdate()
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
    public void ShouldTriggerChangedEventOnEndUpdate()
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
    public async Task ShouldTriggerChangedEventOnOuterEndUpdateAsync()
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
    public void ShouldIncreaseUpdateLockOnBeginUpdate()
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
    public void ShouldDecreaseUpdateLockOnEndUpdate()
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
    public void AssertChangedEventArgs()
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
    public void ShouldNotTriggerChangedWhenSameItemIsAddedThenRemoved()
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
    public void ShouldNotTriggerChangedWhenSameItemIsRemovedThenAdded()
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
