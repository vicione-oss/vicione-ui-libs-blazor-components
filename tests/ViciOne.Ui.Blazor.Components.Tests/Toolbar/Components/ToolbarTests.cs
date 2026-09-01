using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Services;
using ViciOne.Ui.Blazor.Components.Toolbar.Components;
using ToolbarComponent = ViciOne.Ui.Blazor.Components.Toolbar.Components.Toolbar;

namespace ViciOne.Ui.Blazor.Components.Tests.Toolbar.Components;

public sealed class ToolbarTests : IDisposable
{
    private readonly IResizeObserver _resizeObserver;
    private readonly BunitContext _testContext;

    private readonly List<ElementReference> _elementReferences = [];

    public ToolbarTests()
    {
        _resizeObserver = Substitute.For<IResizeObserver>();

        _testContext = new BunitContext();
        _testContext.Services.AddScoped(_ => _resizeObserver);

        _resizeObserver.ObserveAsync(Arg.Do<ElementReference>(_elementReferences.Add), Arg.Any<bool>());
    }

    public void Dispose()
        => _testContext.Dispose();

    [Fact]
    public void Should_render_without_content()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>();

        // Assert
        renderedComponent.Should().NotBeNull();
    }

    [Fact]
    public void Should_render_with_basic_content()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>(b => b
            .AddChildContent<ToolbarButton>()
            .AddChildContent<ToolbarButton>()
            .AddChildContent<ToolbarButton>());

        // Assert
        renderedComponent.FindComponents<ToolbarButton>().Should().HaveCount(3);
    }

    [Fact]
    public void Should_render_with_full_content()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>(b => b
            .AddChildContent<ToolbarButton>()
            .AddChildContent<ToolbarGroup>()
            .AddChildContent<ToolbarGroup>(b => b
                .AddChildContent<ToolbarButton>())
            .AddChildContent<ToolbarGroup>(b => b
                .AddChildContent<ToolbarButton>().AddChildContent<ToolbarButton>())
            .AddChildContent<ToolbarButton>()
        );

        // Assert
        renderedComponent.FindComponents<ToolbarGroup>().Should().HaveCount(3);
        renderedComponent.FindComponents<ToolbarButton>().Should().HaveCount(5);
    }

    [Fact]
    public void Should_render_with_stacked_groups()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>(b => b
            .AddChildContent<ToolbarGroup>(b => b
                .AddChildContent<ToolbarGroup>(b => b
                    .AddChildContent<ToolbarButton>())));

        // Assert
        renderedComponent.FindComponents<ToolbarGroup>().Should().HaveCount(2);
        renderedComponent.FindComponents<ToolbarButton>().Should().HaveCount(1);
    }

    [Fact]
    public void Should_hide_empty_group()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>(b => b.AddChildContent<ToolbarGroup>());

        SetContainerSize(_elementReferences[1], 0);
        SetContainerSize(_elementReferences[0], 10);

        renderedComponent.WaitForState(() => renderedComponent.Markup.Contains("hidden", StringComparison.InvariantCulture));

        // Assert
        renderedComponent.FindComponents<ToolbarGroup>().Should().HaveCount(1);

        renderedComponent.Find(".toolbar-group").ClassList.Should().Contain("hidden");
    }

    [Fact]
    public void Should_hide_toolbar_item_when_not_enough_space()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>(b => b.AddChildContent<ToolbarButton>());

        SetContainerSize(_elementReferences[1], 11);
        SetContainerSize(_elementReferences[0], 10);

        renderedComponent.WaitForState(() => renderedComponent.Markup.Contains("hidden", StringComparison.InvariantCulture));

        // Assert
        renderedComponent.Find(".toolbar-button").ClassList.Should().Contain("hidden");
    }

    [Fact]
    public void Should_hide_group_when_all_children_are_hidden()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>(b => b
            .AddChildContent<ToolbarGroup>(b => b
                .AddChildContent<ToolbarButton>()));

        SetContainerSize(_elementReferences[2], 20);
        SetContainerSize(_elementReferences[1], 0);
        SetContainerSize(_elementReferences[0], 10);

        renderedComponent.WaitForState(() => renderedComponent.Find(".toolbar-group").ClassList.Contains("hidden"));

        // Assert
        renderedComponent.Find(".toolbar-group").ClassList.Should().Contain("hidden");
        renderedComponent.Find(".toolbar-button").ClassList.Should().Contain("hidden");
    }

    [Fact]
    public void Should_consider_item_margin_when_calculating_visibility()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>(b => b
            .AddChildContent<ToolbarButton>()
            .AddChildContent<ToolbarButton>());

        SetContainerSize(_elementReferences[2], 100, marginX: 1);
        SetContainerSize(_elementReferences[1], 100, marginX: 0);
        SetContainerSize(_elementReferences[0], 200);

        renderedComponent.WaitForState(() => renderedComponent.FindAll(".toolbar-button")[^1].ClassList.Contains("hidden"));

        // Assert
        var items = renderedComponent.FindAll(".toolbar-button");
        items[0].ClassList.Should().NotContain("hidden");
        items[1].ClassList.Should().Contain("hidden");
    }

    [Fact]
    public void Should_consider_group_space_requirement_when_calculating_item_visibility()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>(b => b
            .AddChildContent<ToolbarGroup>(b => b
                .AddChildContent<ToolbarButton>()
                .AddChildContent<ToolbarButton>()));

        SetContainerSize(_elementReferences[3], 95, marginX: 0);
        SetContainerSize(_elementReferences[2], 100, marginX: 0);
        SetContainerSize(_elementReferences[1], 999, paddingX: 1, marginX: 1, borderX: 1);
        SetContainerSize(_elementReferences[0], 200);

        renderedComponent.WaitForState(() => renderedComponent.FindAll(".toolbar-button")[^1].ClassList.Contains("hidden"));

        // Assert
        var items = renderedComponent.FindAll(".toolbar-button");
        items[0].ClassList.Should().NotContain("hidden");
        items[1].ClassList.Should().Contain("hidden");
    }

    [Fact]
    public void Should_not_have_menu_when_no_item_is_hidden()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>(b => b
            .AddChildContent<ToolbarButton>()
            .AddChildContent<ToolbarButton>());

        SetContainerSize(_elementReferences[2], 50);
        SetContainerSize(_elementReferences[1], 50);
        SetContainerSize(_elementReferences[0], 100);

        // can not wait for a specific state since "nothing will change" but still need to give enough (> debounce time) time to make sure it's correct
        Thread.Sleep(30);

        // Assert
        renderedComponent.FindAll(".menu-container").Should().HaveCount(0);
    }

    [Fact]
    public void Should_have_menu_when_at_least_one_item_is_hidden()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>(b => b
            .AddChildContent<ToolbarButton>()
            .AddChildContent<ToolbarButton>());

        SetContainerSize(_elementReferences[2], 60);
        SetContainerSize(_elementReferences[1], 50);
        SetContainerSize(_elementReferences[0], 100);

        renderedComponent.WaitForState(() => renderedComponent.FindAll(".menu-container").Any());

        // Assert
        renderedComponent.FindAll(".menu-container").Should().HaveCount(1);
    }

    [Fact]
    public void Should_show_hidden_item_in_menu()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>(b => b
            .AddChildContent<ToolbarButton>());

        SetContainerSize(_elementReferences[1], 101);
        SetContainerSize(_elementReferences[0], 100);

        renderedComponent.WaitForState(() => renderedComponent.FindAll(".menu-container").Any());

        renderedComponent.Find(".menu-container").Click();

        renderedComponent.WaitForState(() => renderedComponent.FindAll(".toolbar-button").Count == 2);

        // Assert
        renderedComponent.FindAll(".toolbar-button").Should().HaveCount(2);
    }


    [Fact]
    public void Should_not_show_invisible_item_in_toolbar()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>(b => b
            .AddChildContent<ToolbarButton>(b => b.Add(x => x.Visible, false)));

        SetContainerSize(_elementReferences[1], 50);
        SetContainerSize(_elementReferences[0], 100);

        renderedComponent.WaitForState(() => renderedComponent.FindAll(".hidden").Count == 1);

        // Assert
        renderedComponent.FindAll(".hidden").Should().HaveCount(1);
    }

    [Fact]
    public void Should_not_show_menu_when_only_containing_invisible_item()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>(b => b
            .AddChildContent<ToolbarButton>(b => b.Add(x => x.Visible, false)));

        SetContainerSize(_elementReferences[1], 101);
        SetContainerSize(_elementReferences[0], 100);

        // can not wait for a specific state since "nothing will change" but still need to give enough (> debounce time) time to make sure it's correct
        Thread.Sleep(30);

        // Assert
        renderedComponent.FindAll(".menu-container").Should().HaveCount(0);
    }

    [Fact]
    public void Should_show_menu_when_only_contained_item_is_changed_to_visible()
    {
        // Act
        var renderedComponent = _testContext.Render<ToolbarComponent>(b => b
            .AddChildContent<ToolbarButton>(b => b.Add(x => x.Visible, false)));

        var itemRef = renderedComponent.FindComponent<ToolbarButton>();

        SetContainerSize(_elementReferences[1], 101);
        SetContainerSize(_elementReferences[0], 100);

        itemRef.Render(p => p.Add(x => x.Visible, true));

        renderedComponent.WaitForState(() => renderedComponent.FindAll(".menu-container").Any());

        // Assert
        renderedComponent.FindAll(".menu-container").Should().HaveCount(1);
    }

    private void SetContainerSize(ElementReference elementReference, int width = 0, int paddingX = 0, int marginX = 0, int borderX = 0)
        => _resizeObserver.ElementSizeChanged += Raise.Event<Action<ElementSizeChangedEventArgs>>(
            new ElementSizeChangedEventArgs
            {
                ElementReference = elementReference,
                DomRect = new DomRect { Width = width, Height = 50 },
                Style = new CssStyleDeclaration
                {
                    BorderLeft = borderX,
                    BorderRight = borderX,
                    MarginLeft = marginX,
                    MarginRight = marginX,
                    PaddingLeft = paddingX,
                    PaddingRight = paddingX
                }
            }
        );
}
