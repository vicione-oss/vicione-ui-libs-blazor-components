using AwesomeAssertions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Services;

public sealed class PropertyEntryContextMenuStateTests
{
    [Fact]
    public void Should_be_resolvable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var serviceKey = typeof(object);

        // Act
        var state = serviceProvider.GetKeyedService<PropertyEntryContextMenuState>(serviceKey);

        // Assert
        state.Should().NotBeNull();
    }

    [Fact]
    public void Should_set_properties_on_update()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPropertyGrid<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var serviceKey = typeof(object);
        var state = serviceProvider.GetRequiredKeyedService<PropertyEntryContextMenuState>(serviceKey);

        var propertyGridItem = Substitute.For<IPropertyGridItem>();
        var context = new PropertyEntryContextMenuContext { MouseEventArgs = new MouseEventArgs(), PropertyGridItem = propertyGridItem };

        // Act
        state.Update(context);

        // Assert
        state.HeaderText.Should().Be(context.PropertyGridItem.DisplayName);
        state.ResetVisible.Should().Be(context.PropertyGridItem.Resettable);
        state.SetToNullVisible.Should().Be(context.PropertyGridItem.CanBeSetToNull);
    }
}
