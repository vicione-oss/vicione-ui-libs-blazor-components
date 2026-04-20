using AwesomeAssertions;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Components;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.Tooltip.Extensions;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Components;

public sealed partial class PropertyGridTests
{
    public sealed class InsertionOrderPropertyComparer
    {
        [Fact]
        public void Should_preserve_insertion_order_in_flat_mode()
        {
            // Arrange
            using var testContext = CreatePropertyEntrySetTestContext();
            var controller = Substitute.For<IPropertyGridController>();
            IPropertyGridItem[] items =
            [
                CreatePropertyGridItem("Zebra"),
                CreatePropertyGridItem("Alpha"),
                CreatePropertyGridItem("Mango"),
            ];

            // Act
            var component = testContext.Render<PropertyEntrySet>(b => b
                .AddCascadingValue(controller)
                .Add(p => p.Items, items)
                .Add(p => p.PropertyComparer, testContext.Services.GetRequiredService<IInsertionOrderPropertyComparer>()));

            // Assert
            var names = component.FindAll(".property-name").Select(e => e.TextContent.Trim()).ToList();
            names.Should().Equal("Zebra", "Alpha", "Mango");
        }
        private static BunitContext CreatePropertyEntrySetTestContext()
        {
            var context = new BunitContext();
            context.Services.AddTooltip();
            context.Services.AddCategoryComparers().AddPropertyComparers();
            context.Services.AddScoped(_ => Substitute.For<IPropertyEditorComponentRegistry>());
            return context;
        }
    }
}
