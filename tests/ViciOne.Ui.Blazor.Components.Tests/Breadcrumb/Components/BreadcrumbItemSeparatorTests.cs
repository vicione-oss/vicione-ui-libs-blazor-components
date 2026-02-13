using AwesomeAssertions;
using Bunit;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Components;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Models;
using Xunit;
using TestContext = Bunit.TestContext;

namespace ViciOne.Ui.Blazor.Components.Tests.Breadcrumb.Components;

public sealed class BreadcrumbItemSeparatorTests
{
    [Fact]
    public void Should_expand_on_click()
    {
        // Arrange
        List<BreadcrumbItem> items = [new() { Name = "A" }, new() { Name = "B" }];

        using var testContext = new TestContext();
        var renderedComponent = testContext.RenderComponent<BreadcrumbItemSeparator>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.ActiveItem, items[0]));

        // Act & Assert
        renderedComponent.FindAll(".popup-item").Should().BeEmpty();
        renderedComponent.Find(".icon").Click();
        renderedComponent.FindAll(".popup-item").Should().HaveCount(2);
        renderedComponent.FindAll(".popup-item.active").Should().HaveCount(1);
    }
}
