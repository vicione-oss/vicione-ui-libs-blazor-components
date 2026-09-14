using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.TabStrip.Components;
using ViciOne.Ui.Blazor.Components.TabStrip.Models;
using TabStripComponent = ViciOne.Ui.Blazor.Components.TabStrip.Components.TabStrip;

namespace ViciOne.Ui.Blazor.Components.Tests.TabStrip.Components;

public sealed class TabTests : IAsyncDisposable
{
    private static readonly string s_jsModuleIdentifier =
        $"./_content/{typeof(TabStripComponent).Assembly.GetName().Name}/tab-strip/components/tab-strip.js";

    private readonly BunitContext _testContext;

    public TabTests()
    {
        _testContext = new BunitContext();

        var module = _testContext.JSInterop.SetupModule(s_jsModuleIdentifier);
        var attachResult = module.SetupModule("attach", _ => true);
        attachResult.Mode = JSRuntimeMode.Loose;
    }

    public ValueTask DisposeAsync()
        => _testContext.DisposeAsync();

    private static RenderFragment BuildTabsFragment(params string[] texts)
        => builder =>
        {
            for (var i = 0; i < texts.Length; i++)
            {
                builder.OpenComponent<Tab>(i * 10);
                builder.AddAttribute((i * 10) + 1, nameof(Tab.Text), texts[i]);
                builder.CloseComponent();
            }
        };

    [Fact]
    public void Should_render_root_element_with_tab_class()
    {
        // Act
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "Item")));

        // Assert
        renderedComponent.Find(".tab").Should().NotBeNull();
    }

    [Fact]
    public void Should_render_text()
    {
        // Act
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "General")));

        // Assert
        renderedComponent.Find(".tab .text").TextContent.Should().Be("General");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_render_with_empty_text_content_when_text_is_null_or_empty(string? text)
    {
        // Act
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, text)));

        // Assert
        renderedComponent.Find(".tab .text").TextContent.Should().BeEmpty();
    }

    [Fact]
    public void Should_render_with_active_css_class_when_index_matches_active()
    {
        // Act
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 1)
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "B")));

        var activeTab = renderedComponent.FindAll(".tab")[1];

        // Assert
        activeTab.ClassList.Should().Contain("active");
    }

    [Fact]
    public void Should_not_render_active_css_class_when_index_does_not_match_active()
    {
        // Act
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "B")));

        var inactiveTab = renderedComponent.FindAll(".tab")[1];

        // Assert
        inactiveTab.ClassList.Should().NotContain("active");
    }

    [Fact]
    public void Click_on_tab_should_invoke_on_click_callback()
    {
        // Arrange
        var onClickInvoked = false;

        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "B")
                .Add(p => p.OnSelected, _ => onClickInvoked = true)));

        // Act
        renderedComponent.FindAll(".tab")[1].Click();

        // Assert
        onClickInvoked.Should().BeTrue();
    }

    [Fact]
    public void Click_on_tab_should_pass_tab_index_to_on_click_callback()
    {
        // Arrange
        TabSelectedEventArgs? receivedArgs = null;

        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "B")
                .Add(p => p.OnSelected, args => receivedArgs = args))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "C")));

        // Act
        renderedComponent.FindAll(".tab")[1].Click();

        // Assert
        receivedArgs.Should().NotBeNull();
        receivedArgs!.TabIndex.Should().Be(1);
    }

    [Fact]
    public void Click_on_active_tab_should_not_invoke_on_click_callback()
    {
        // Arrange
        var onClickInvocationCount = 0;

        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "A")
                .Add(p => p.OnSelected, _ => onClickInvocationCount++))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "B")));

        // Act
        renderedComponent.FindAll(".tab")[0].Click();

        // Assert
        onClickInvocationCount.Should().Be(0);
    }

    [Fact]
    public void Click_on_tab_should_make_clicked_tab_active()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "B"))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "C")));

        // Act
        renderedComponent.FindAll(".tab")[2].Click();

        // Assert
        var tabs = renderedComponent.FindAll(".tab");
        tabs[0].ClassList.Should().NotContain("active");
        tabs[1].ClassList.Should().NotContain("active");
        tabs[2].ClassList.Should().Contain("active");
    }

    [Fact]
    public void Click_on_tab_should_deactivate_previously_active_tab()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 1)
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "B"))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "C")));

        // Act
        renderedComponent.FindAll(".tab")[0].Click();

        // Assert
        var tabs = renderedComponent.FindAll(".tab");
        tabs[0].ClassList.Should().Contain("active");
        tabs[1].ClassList.Should().NotContain("active");
    }

    [Fact]
    public void Click_should_invoke_on_click_before_active_tab_index_changed()
    {
        // Arrange
        var sequence = 0;
        var onClickStep = 0;
        var activeIndexChangedStep = 0;

        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .Add(p => p.ActiveTabIndexChanged, _ => activeIndexChangedStep = ++sequence)
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "B")
                .Add(p => p.OnSelected, _ => onClickStep = ++sequence)));

        // Act
        renderedComponent.FindAll(".tab")[1].Click();

        // Assert
        onClickStep.Should().Be(1);
        activeIndexChangedStep.Should().Be(2);
        renderedComponent.Instance.ActiveTabIndex.Should().Be(1);
    }

    [Fact]
    public void Tab_added_after_initial_render_should_be_clickable()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .Add(p => p.ChildContent, BuildTabsFragment("A")));

        // Act
        renderedComponent.Render(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .Add(p => p.ChildContent, BuildTabsFragment("A", "B")));

        renderedComponent.FindAll(".tab")[1].Click();

        // Assert
        renderedComponent.Instance.ActiveTabIndex.Should().Be(1);
    }

    [Fact]
    public void Tab_removed_after_initial_render_should_no_longer_affect_indexing()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .Add(p => p.ChildContent, BuildTabsFragment("A", "B", "C")));

        // Act
        renderedComponent.Render(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .Add(p => p.ChildContent, BuildTabsFragment("A", "C")));

        renderedComponent.FindAll(".tab")[1].Click();

        // Assert
        renderedComponent.Instance.ActiveTabIndex.Should().Be(1);
        renderedComponent.FindAll(".tab").Should().HaveCount(2);
    }

    [Theory]
    [InlineData("Enter")]
    [InlineData("NumpadEnter")]
    public async Task Enter_on_tab_should_make_tab_active(string key)
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "B"))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "C")));

        // Act
        await renderedComponent.FindAll(".tab")[2].KeyDownAsync(new KeyboardEventArgs { Key = key });

        // Assert
        renderedComponent.Instance.ActiveTabIndex.Should().Be(2);
        var tabs = renderedComponent.FindAll(".tab");
        tabs[0].ClassList.Should().NotContain("active");
        tabs[1].ClassList.Should().NotContain("active");
        tabs[2].ClassList.Should().Contain("active");
    }

    [Fact]
    public async Task Enter_on_tab_should_invoke_on_click_callback()
    {
        // Arrange
        TabSelectedEventArgs? receivedArgs = null;

        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "B")
                .Add(p => p.OnSelected, args => receivedArgs = args)));

        // Act
        await renderedComponent.FindAll(".tab")[1].KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        receivedArgs.Should().NotBeNull();
        receivedArgs!.TabIndex.Should().Be(1);
    }

    [Fact]
    public async Task Enter_on_active_tab_should_not_invoke_on_click_callback()
    {
        // Arrange
        var onClickInvocationCount = 0;

        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "A")
                .Add(p => p.OnSelected, _ => onClickInvocationCount++))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "B")));

        // Act
        await renderedComponent.FindAll(".tab")[0].KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        onClickInvocationCount.Should().Be(0);
    }

    [Fact]
    public async Task Non_enter_key_on_tab_should_not_change_active_tab()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t
                .Add(p => p.Text, "B")));

        // Act
        await renderedComponent.FindAll(".tab")[1].KeyDownAsync(new KeyboardEventArgs { Key = "Space" });

        // Assert
        renderedComponent.Instance.ActiveTabIndex.Should().Be(0);
    }
}
