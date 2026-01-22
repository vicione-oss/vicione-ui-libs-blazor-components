using AwesomeAssertions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.ContextMenu.Services;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Services;

public sealed class PropertyGridControllerTests
{
    private static readonly IPropertyDescriptor<Foo> s_briefPropertyDescriptor = new PropertyDescriptor<Foo, string>
    {
        Name = nameof(Foo.Brief),
        GetValue = instance => instance.Brief
    };

    private static readonly IPropertyDescriptor<Foo> s_abstractPropertyDescriptor = new PropertyDescriptor<Foo, string>
    {
        Name = nameof(Foo.Abstract),
        GetValue = instance => instance.Abstract,
        DependsOn = [s_briefPropertyDescriptor]
    };

    private static readonly TimeSpan s_waitTimeout = TimeSpan.FromMilliseconds(5000);

    [Fact]
    public void Should_be_resolvable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var propertyGridController = serviceProvider.GetService<IPropertyGridController<object>>();

        // Assert
        propertyGridController.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_update_property_grid_state_on_set_instance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>()
            .WithPropertyDescriptorProvider<FooPropertyDescriptorProvider>();

        await using var serviceProvider = services.BuildServiceProvider();

        var propertyGridController = serviceProvider.GetRequiredService<IPropertyGridController<object>>();
        var propretyGridState = serviceProvider.GetRequiredService<IPropertyGridState<object>>();

        var taskCompletionSource = new TaskCompletionSource<bool>();
        propretyGridState.PropertiesChanged += _ => taskCompletionSource.SetResult(true);

        var instance = new Foo();
        var context = new object();

        // Act
        propertyGridController.SetInstances([instance], context);

        var propretyGridStateChanged = await taskCompletionSource.Task.WaitAsync(s_waitTimeout, CancellationToken.None);

        // Assert
        propretyGridStateChanged.Should().BeTrue();

        propretyGridState.Items.Should().HaveCount(2);

        propretyGridState.Items.SelectMany(i => i.PropertyDescriptors).Should()
            .BeEquivalentTo([s_briefPropertyDescriptor, s_abstractPropertyDescriptor]);

        propretyGridState.ItemDependentsMap.Should().HaveCount(1);

        var firstKeyValuePair = propretyGridState.ItemDependentsMap.First();
        firstKeyValuePair.Key.PropertyDescriptors.Should().BeEquivalentTo([s_briefPropertyDescriptor]);
        firstKeyValuePair.Value.SelectMany(propertyGridItem => propertyGridItem.PropertyDescriptors)
            .Should().BeEquivalentTo([s_abstractPropertyDescriptor]);
    }

    [Fact]
    public async Task Should_raise_event_on_update_property()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>()
            .WithPropertyDescriptorProvider<FooPropertyDescriptorProvider>();

        await using var serviceProvider = services.BuildServiceProvider();

        var propertyGridController = serviceProvider.GetRequiredService<IPropertyGridController<object>>();
        var propretyGridState = serviceProvider.GetRequiredService<IPropertyGridState<object>>();

        var propretyGridStateChangedTaskCompletionSource = new TaskCompletionSource<bool>();
        propretyGridState.PropertiesChanged += _ => propretyGridStateChangedTaskCompletionSource.SetResult(true);

        var updatePropertyRequestedTaskCompletionSource
            = new TaskCompletionSource<PropertyGridControllerUpdatePropertyRequestedEventArgs?>();
        propertyGridController.UpdatePropertyRequested += updatePropertyRequestedTaskCompletionSource.SetResult;

        var instance = new Foo();
        var context = new object();

        // Act
        propertyGridController.SetInstances([instance], context);

        var propretyGridStateChanged = await propretyGridStateChangedTaskCompletionSource.Task
            .WaitAsync(s_waitTimeout, CancellationToken.None);

        propertyGridController.UpdateProperty(nameof(Foo.Brief));
        propertyGridController.UpdateProperty(nameof(Foo.Abstract));

        var updatePropertyRequestedEventArgs = await updatePropertyRequestedTaskCompletionSource.Task
            .WaitAsync(s_waitTimeout, CancellationToken.None);

        // Assert
        propretyGridStateChanged.Should().BeTrue();
        updatePropertyRequestedEventArgs.Should().NotBeNull();

        updatePropertyRequestedEventArgs!.Targets.SelectMany(target => target.PropertyDescriptors)
            .Should().BeEquivalentTo([s_briefPropertyDescriptor, s_abstractPropertyDescriptor]);
    }

    [Fact]
    public void Should_raise_event_on_focus_property()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var propertyGridController = serviceProvider.GetRequiredService<IPropertyGridController<object>>();

        PropertyGridControllerFocusPropertyRequestedEventArgs? focusPropertyRequestedEventArgs = null;
        propertyGridController.FocusPropertyRequested += args => focusPropertyRequestedEventArgs = args;

        const string PropertyName = nameof(Foo.Brief);

        // Act
        propertyGridController.FocusProperty(PropertyName);

        // Assert
        focusPropertyRequestedEventArgs.Should().NotBeNull();
        focusPropertyRequestedEventArgs!.Name.Should().Be(PropertyName);
    }

    [Fact]
    public async Task Should_send_context_menu_request_on_show_context_menu_async()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        await using var serviceProvider = services.BuildServiceProvider();

        var propertyGridController = serviceProvider.GetRequiredService<IPropertyGridController<object>>();

        var contextMenuRequestServiceKey = typeof(object);
        var contextMenuRequest = serviceProvider
            .GetRequiredKeyedService<IContextMenuRequest<PropertyEntryContextMenuContext>>(contextMenuRequestServiceKey);

        var contextMenuRequested = false;
        PropertyEntryContextMenuContext? contextMenuRequestedContext = null;

        contextMenuRequest.ContextMenuRequestedAsync += context =>
        {
            contextMenuRequested = true;
            contextMenuRequestedContext = context;

            return Task.CompletedTask;
        };

        var contextMenuContext = new PropertyEntryContextMenuContext
        {
            MouseEventArgs = new MouseEventArgs(),
            PropertyGridItem = Substitute.For<IPropertyGridItem>()
        };

        // Act
        await propertyGridController.ShowContextMenuAsync(contextMenuContext);

        // Assert
        contextMenuRequested.Should().BeTrue();
        contextMenuRequestedContext.Should().Be(contextMenuContext);
    }

    [Fact]
    public async Task Should_raise_event_on_update_dependents()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>()
            .WithPropertyDescriptorProvider<FooPropertyDescriptorProvider>();

        await using var serviceProvider = services.BuildServiceProvider();

        var propertyGridController = serviceProvider.GetRequiredService<IPropertyGridController<object>>();

        PropertyGridControllerUpdatePropertyRequestedEventArgs? updatePropertyRequestedEventArgs = null;
        propertyGridController.UpdatePropertyRequested += args => updatePropertyRequestedEventArgs = args;

        var propretyGridState = serviceProvider.GetRequiredService<IPropertyGridState<object>>();

        var propretyGridStateChangedTaskCompletionSource = new TaskCompletionSource<bool>();
        propretyGridState.PropertiesChanged += _ => propretyGridStateChangedTaskCompletionSource.SetResult(true);

        var instance = new Foo();
        var context = new object();

        // Act, Assert
        propertyGridController.SetInstances([instance], context);

        var propretyGridStateChanged = await propretyGridStateChangedTaskCompletionSource.Task
            .WaitAsync(s_waitTimeout, CancellationToken.None);
        propretyGridStateChanged.Should().BeTrue();

        var briefPropertyGridItem = propretyGridState.Items.First(i => i.PropertyDescriptors.Contains(s_briefPropertyDescriptor));
        propertyGridController.UpdateDependents(briefPropertyGridItem);

        updatePropertyRequestedEventArgs.Should().NotBeNull();
        updatePropertyRequestedEventArgs!.Targets.SelectMany(target => target.PropertyDescriptors)
            .Should().BeEquivalentTo([s_abstractPropertyDescriptor]);
    }

    private sealed class Foo
    {
        public string Brief { get; set; } = string.Empty;
        public string Abstract { get; set; } = string.Empty;
    }

    private sealed class FooPropertyDescriptorProvider : IPropertyDescriptorProvider<object, Foo>
    {
        public IEnumerable<IPropertyDescriptor<Foo>> GetPropertyDescriptors(object context)
        {
            yield return s_briefPropertyDescriptor;
            yield return s_abstractPropertyDescriptor;
        }
    }
}
