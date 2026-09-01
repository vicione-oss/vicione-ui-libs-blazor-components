using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Messages;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Services;

public sealed class PropertyGridMessageStoreTests
{
    [Fact]
    public void Should_be_resolvable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var errorMessageStore = serviceProvider.GetService<IPropertyGridMessageStore<object>>();

        // Assert
        errorMessageStore.Should().NotBeNull();
    }

    [Fact]
    public void Should_have_count_of_one()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var errorMessageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();

        // Act
        errorMessageStore.Add(Substitute.For<IPropertyGridItem>(), new ErrorMessage { Text = "An error has occurred." });

        // Assert
        errorMessageStore.Count.Should().Be(1);
    }

    [Fact]
    public void Should_hold_errors_associated_with_item()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var errorMessageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();

        var propertyGridItem = Substitute.For<IPropertyGridItem>();

        var errorMessage1 = new ErrorMessage { Text = "An error has occurred." };
        errorMessageStore.Add(propertyGridItem, errorMessage1);

        var errorMessage2 = new ErrorMessage { Text = "Another error has occurred." };
        errorMessageStore.Add(propertyGridItem, errorMessage2);

        // Act
        var errorMessages = errorMessageStore.Get(propertyGridItem);

        // Assert
        errorMessages.Should().BeEquivalentTo([errorMessage1, errorMessage2]);
    }

    [Fact]
    public void Should_remove_errors_associated_with_item()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var errorMessageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();

        var propertyGridItem = Substitute.For<IPropertyGridItem>();

        var errorMessage1 = new ErrorMessage { Text = "An error has occurred." };
        errorMessageStore.Add(propertyGridItem, errorMessage1);

        var errorMessage2 = new ErrorMessage { Text = "Another error has occurred." };
        errorMessageStore.Add(propertyGridItem, errorMessage2);

        // Act
        errorMessageStore.Remove(propertyGridItem);

        var errorMessages = errorMessageStore.Get(propertyGridItem);

        // Assert
        errorMessages.Should().BeEmpty();
    }

    [Fact]
    public void Should_clear_all_errors()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var errorMessageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();

        var propertyGridItem = Substitute.For<IPropertyGridItem>();

        var errorMessage1 = new ErrorMessage { Text = "An error has occurred." };
        errorMessageStore.Add(propertyGridItem, errorMessage1);

        var errorMessage2 = new ErrorMessage { Text = "Another error has occurred." };
        errorMessageStore.Add(propertyGridItem, errorMessage2);

        // Act
        errorMessageStore.Clear();

        var errorMessages = errorMessageStore.Get(propertyGridItem);

        // Assert
        errorMessages.Should().BeEmpty();
        errorMessageStore.Count.Should().Be(0);
    }

    [Fact]
    public void Should_trigger_changed_event_on_add_operation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var errorMessageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();
        var errorMessageStoreMonitor = errorMessageStore.Monitor();

        var propertyGridItem = Substitute.For<IPropertyGridItem>();

        // Act
        errorMessageStore.Add(propertyGridItem, new ErrorMessage { Text = "An error has occurred." });

        // Assert
        errorMessageStoreMonitor.Should().Raise(nameof(errorMessageStore.Changed));
    }

    [Fact]
    public void Should_trigger_changed_event_on_clear_operation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var errorMessageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();
        errorMessageStore.Add(Substitute.For<IPropertyGridItem>(), new ErrorMessage { Text = "An error has occurred." });

        var errorMessageStoreMonitor = errorMessageStore.Monitor();

        // Act
        errorMessageStore.Clear();

        // Assert
        errorMessageStoreMonitor.Should().Raise(nameof(errorMessageStore.Changed));
    }

    [Fact]
    public void Should_not_trigger_changed_event_after_begin_update()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var errorMessageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();
        var errorMessageStoreMonitor = errorMessageStore.Monitor();

        // Act
        errorMessageStore.BeginUpdate();
        errorMessageStore.Add(Substitute.For<IPropertyGridItem>(), new ErrorMessage { Text = "An error has occurred." });
        errorMessageStore.Clear();

        // Assert
        errorMessageStoreMonitor.Should().NotRaise(nameof(errorMessageStore.Changed));
    }

    [Fact]
    public void Should_trigger_changed_event_after_end_update()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var errorMessageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();
        var errorMessageStoreMonitor = errorMessageStore.Monitor();

        var propertyGridItem = Substitute.For<IPropertyGridItem>();

        // Act
        errorMessageStore.BeginUpdate();
        try
        {
            errorMessageStore.Add(propertyGridItem, new ErrorMessage { Text = "An error has occurred." });
            errorMessageStore.Remove(propertyGridItem);

            errorMessageStore.Add(propertyGridItem, new ErrorMessage { Text = "An error has occurred." });
            errorMessageStore.Clear();

            errorMessageStore.Add(propertyGridItem, new ErrorMessage { Text = "An error has occurred." });
        }
        finally
        {
            errorMessageStore.EndUpdate();
        }

        // Assert
        errorMessageStoreMonitor.Should().Raise(nameof(errorMessageStore.Changed));
    }

    [Fact]
    public void Should_not_trigger_changed_event_when_remove_operation_cancels_add_operation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var errorMessageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();
        var errorMessageStoreMonitor = errorMessageStore.Monitor();

        var propertyGridItem = Substitute.For<IPropertyGridItem>();

        // Act
        errorMessageStore.BeginUpdate();
        try
        {
            errorMessageStore.Add(propertyGridItem, new ErrorMessage { Text = "An error has occurred." });
            errorMessageStore.Remove(propertyGridItem);
        }
        finally
        {
            errorMessageStore.EndUpdate();
        }

        // Assert
        errorMessageStoreMonitor.Should().NotRaise(nameof(errorMessageStore.Changed));
    }

    [Fact]
    public void Should_not_trigger_changed_event_when_clear_operation_cancels_add_operation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var errorMessageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();
        var errorMessageStoreMonitor = errorMessageStore.Monitor();

        // Act
        errorMessageStore.BeginUpdate();
        try
        {
            errorMessageStore.Add(Substitute.For<IPropertyGridItem>(), new ErrorMessage { Text = "An error has occurred." });
            errorMessageStore.Clear();
        }
        finally
        {
            errorMessageStore.EndUpdate();
        }

        // Assert
        errorMessageStoreMonitor.Should().NotRaise(nameof(errorMessageStore.Changed));
    }

    [Fact]
    public async Task Should_trigger_changed_event_on_outer_end_update()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        await using var serviceProvider = services.BuildServiceProvider();

        var errorMessageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();

        var errorMessageStoreChangedCounter = 0;

        errorMessageStore.Changed += _ => ++errorMessageStoreChangedCounter;

        static async Task RandomUpdateTaskAsync(IPropertyGridMessageStore<object> errorMessageStore)
        {
            var delay = RandomNumberGenerator.GetInt32(100);
            await Task.Delay(delay);

            var propertyGridItem = Substitute.For<IPropertyGridItem>();

            errorMessageStore.BeginUpdate();
            try
            {
                var coinToss = RandomNumberGenerator.GetInt32(2);

                if (coinToss == 1)
                    errorMessageStore.Add(propertyGridItem, new ErrorMessage { Text = "An error has occurred." });
                else
                    errorMessageStore.Remove(propertyGridItem);
            }
            finally
            {
                errorMessageStore.EndUpdate();
            }
        }

        // Act
        errorMessageStore.BeginUpdate();
        try
        {
            var updateTasks = new List<Task>();
            for (var i = 0; i < 1000; i++)
                updateTasks.Add(RandomUpdateTaskAsync(errorMessageStore));

            await Task.WhenAll(updateTasks);

            // There is a chance that all tasks above add and remove the same item, hence no change would be raised.
            // So, we add an additional item with a nested BeginUpdate / EndUpdate here to ensure Change event is triggered
            errorMessageStore.BeginUpdate();
            try
            {
                var propertyGridItem = Substitute.For<IPropertyGridItem>();

                errorMessageStore.Add(propertyGridItem, new ErrorMessage { Text = "Another error has occurred." });
            }
            finally
            {
                errorMessageStore.EndUpdate();
            }
        }
        finally
        {
            errorMessageStore.EndUpdate();
        }

        // Assert
        errorMessageStoreChangedCounter.Should().Be(1);
    }

    [Fact]
    public void Should_increase_update_lock_on_begin_update()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var errorMessageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();

        // Act
        errorMessageStore.BeginUpdate();

        // Assert
        errorMessageStore.UpdateLock.Should().Be(1);
    }

    [Fact]
    public void Should_decrease_update_lock_on_end_update()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var errorMessageStore = serviceProvider.GetRequiredService<IPropertyGridMessageStore<object>>();

        // Act
        errorMessageStore.BeginUpdate();
        errorMessageStore.EndUpdate();

        // Assert
        errorMessageStore.UpdateLock.Should().Be(0);
    }
}
