using AwesomeAssertions;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Components;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Extensions;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Models;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Services;
using ViciOne.Ui.Blazor.Components.Resizing.Services;
using Xunit;
using BreadcrumbComponent = ViciOne.Ui.Blazor.Components.Breadcrumb.Components.Breadcrumb;
using TestContext = Bunit.TestContext;

namespace ViciOne.Ui.Blazor.Components.Tests.Breadcrumb.Components;

public sealed class BreadcrumbTests
{
    [Fact]
    public void Should_render_no_separator_if_item_has_no_child()
    {
        // Arrange
        var item = new BreadcrumbItem { Name = "root" };

        using var testContext = new TestContext();
        testContext.Services.AddBreadcrumb()
            .AddScoped(_ => Substitute.For<IResizeObserver>())
            .AddScoped(_ => Substitute.For<IHtmlElementHelper>());

        // Act
        var renderedComponent = testContext.RenderComponent<BreadcrumbComponent>(
            b => b.Add(p => p.CurrentItem, item));

        // Assert
        renderedComponent.FindComponents<BreadcrumbItemSeparator>().Should().HaveCount(0);
    }

    [Fact]
    public void Should_render_separator_if_item_has_child()
    {
        // Arrange
        var item = new BreadcrumbItem { Name = "root" };
        var child = new BreadcrumbItem { Name = "child" };
        item.AddChild(child);

        using var testContext = new TestContext();
        testContext.Services.AddBreadcrumb()
            .AddScoped(_ => Substitute.For<IResizeObserver>())
            .AddScoped(_ => Substitute.For<IHtmlElementHelper>());

        // Act
        var renderedComponent = testContext.RenderComponent<BreadcrumbComponent>(
            b => b.Add(p => p.CurrentItem, item));

        // Assert
        renderedComponent.FindComponents<BreadcrumbItemSeparator>().Should().HaveCount(1);
    }
}
