using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Factories;
using ViciOne.Ui.Blazor.Components.TabStrip.Components;
using ViciOne.Ui.Blazor.Components.TabStrip.Enums;
using ViciOne.Ui.Blazor.Components.TabStrip.Extensions;
using TabStripComponent = ViciOne.Ui.Blazor.Components.TabStrip.Components.TabStrip;

namespace ViciOne.Ui.Blazor.Components.Tests.TabStrip.Components;

public sealed partial class TabStripTests : IAsyncDisposable
{
    public static readonly TheoryData<string> TabSizes =
        [.. TypeSafeEnumFactory<TabSize>.CreateAll().Select(size => size.GetName())];

    private static readonly string s_jsModuleIdentifier =
        $"./_content/{typeof(TabStripComponent).Assembly.GetName().Name}/tab-strip/components/tab-strip.js";

    private readonly BunitContext _testContext;
    private readonly BunitJSModuleInterop _jsModule;
    private readonly BunitJSModuleInterop _jsAttachResult;

    public TabStripTests()
    {
        _testContext = new BunitContext();

        _jsModule = _testContext.JSInterop.SetupModule(s_jsModuleIdentifier);
        _jsAttachResult = _jsModule.SetupModule("attach", _ => true);
        _jsAttachResult.Mode = JSRuntimeMode.Loose;
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
    public void Should_render_root_element_with_tab_strip_class()
    {
        // Act
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ChildContent, _ => { }));

        // Assert
        renderedComponent.Find(".tab-strip").Should().NotBeNull();
    }

    [Fact]
    public void Should_render_with_small_size_modifier_css_class_by_default()
    {
        // Act
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ChildContent, _ => { }));

        var root = renderedComponent.Find(".tab-strip");

        // Assert
        root.ClassList.Should().Contain(TabSize.Small.ToModifierCssClass());
    }

    [Theory]
    [MemberData(nameof(TabSizes))]
    public void Should_render_tab_size_modifier_css_class(string tabSize)
    {
        // Arrange
        var tabSizeTyped = TypeSafeEnumFactory<TabSize>.Create(tabSize);

        // Act
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.TabSize, tabSizeTyped)
            .Add(p => p.ChildContent, _ => { }));

        var root = renderedComponent.Find(".tab-strip");

        // Assert
        root.ClassList.Should().Contain(tabSizeTyped.ToModifierCssClass());
    }

    [Fact]
    public void Should_render_tabs_when_provided_as_child_content()
    {
        // Act
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "First"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "Second"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "Third")));

        // Assert
        renderedComponent.FindComponents<Tab>().Should().HaveCount(3);

        var tabTexts = renderedComponent.FindAll(".tab .text").Select(e => e.TextContent);
        tabTexts.Should().Equal("First", "Second", "Third");
    }

    [Fact]
    public void Should_not_render_scroll_icons_when_not_scrollable()
    {
        // Act
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ChildContent, _ => { }));

        // Assert
        renderedComponent.FindAll(".scroll-icon").Should().BeEmpty();
        renderedComponent.Find(".tab-strip").ClassList.Should().NotContain("has-scroll");
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Should_render_scroll_icons_when_scrollable(bool canScrollLeft, bool canScrollRight)
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ChildContent, _ => { }));

        // Act
        await renderedComponent.Instance.ScrollStateChangedAsync(canScrollLeft, canScrollRight);

        // Assert
        renderedComponent.Find(".tab-strip").ClassList.Should().Contain("with-scrolling");
        renderedComponent.FindAll(".tab-strip-scroll-button").Should().HaveCount(2);
    }

    [Fact]
    public async Task Should_mark_left_scroll_icon_active_when_can_scroll_left()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ChildContent, _ => { }));

        // Act
        await renderedComponent.Instance.ScrollStateChangedAsync(canScrollLeft: true, canScrollRight: false);

        // Assert
        var scrollButtons = renderedComponent.FindAll(".tab-strip-scroll-button");
        scrollButtons[0].ClassList.Should().Contain("active");
        scrollButtons[1].ClassList.Should().NotContain("active");
    }

    [Fact]
    public async Task Should_mark_right_scroll_icon_active_when_can_scroll_right()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ChildContent, _ => { }));

        // Act
        await renderedComponent.Instance.ScrollStateChangedAsync(canScrollLeft: false, canScrollRight: true);

        // Assert
        var scrollButtons = renderedComponent.FindAll(".tab-strip-scroll-button");
        scrollButtons[1].ClassList.Should().Contain("active");
        scrollButtons[0].ClassList.Should().NotContain("active");
    }

    [Fact]
    public async Task Click_on_active_scroll_left_should_invoke_js_scroll_left()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ChildContent, _ => { }));

        await renderedComponent.Instance.ScrollStateChangedAsync(canScrollLeft: true, canScrollRight: false);

        // Act
        await renderedComponent.Find(".tab-strip-scroll-button").ClickAsync();

        // Assert
        _jsAttachResult.VerifyInvoke("scrollLeft");
    }

    [Fact]
    public async Task Click_on_active_scroll_right_should_invoke_js_scroll_right()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ChildContent, _ => { }));

        await renderedComponent.Instance.ScrollStateChangedAsync(canScrollLeft: false, canScrollRight: true);

        // Act
        await renderedComponent.FindAll(".tab-strip-scroll-button")[1].ClickAsync();

        // Assert
        _jsAttachResult.VerifyInvoke("scrollRight");
    }

    [Fact]
    public async Task Click_on_inactive_scroll_left_should_not_invoke_js_scroll_left()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ChildContent, _ => { }));

        await renderedComponent.Instance.ScrollStateChangedAsync(canScrollLeft: false, canScrollRight: true);

        // Act
        await renderedComponent.FindAll(".tab-strip-scroll-button")[0].ClickAsync();

        // Assert
        _jsAttachResult.VerifyNotInvoke("scrollLeft");
    }

    [Fact]
    public async Task Click_on_inactive_scroll_right_should_not_invoke_js_scroll_right()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ChildContent, _ => { }));

        await renderedComponent.Instance.ScrollStateChangedAsync(canScrollLeft: true, canScrollRight: false);

        // Act
        await renderedComponent.FindAll(".tab-strip-scroll-button")[1].ClickAsync();

        // Assert
        _jsAttachResult.VerifyNotInvoke("scrollRight");
    }

    [Fact]
    public void Should_render_tab_at_given_active_index_as_active()
    {
        // Act
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 1)
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "B"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "C")));

        var tabs = renderedComponent.FindAll(".tab");

        // Assert
        tabs[0].ClassList.Should().NotContain("active");
        tabs[1].ClassList.Should().Contain("active");
        tabs[2].ClassList.Should().NotContain("active");
    }

    [Fact]
    public void Tab_click_should_update_active_tab_index()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "B"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "C")));

        // Act
        renderedComponent.FindAll(".tab")[2].Click();

        // Assert
        renderedComponent.Instance.ActiveTabIndex.Should().Be(2);

        var tabs = renderedComponent.FindAll(".tab");
        tabs[0].ClassList.Should().NotContain("active");
        tabs[2].ClassList.Should().Contain("active");
    }

    [Fact]
    public void Tab_click_should_invoke_active_tab_index_changed_callback()
    {
        // Arrange
        int? receivedIndex = null;

        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .Add(p => p.ActiveTabIndexChanged, index => receivedIndex = index)
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "B")));

        // Act
        renderedComponent.FindAll(".tab")[1].Click();

        // Assert
        receivedIndex.Should().Be(1);
    }

    [Fact]
    public void Tab_click_on_active_tab_should_not_invoke_active_tab_index_changed_callback()
    {
        // Arrange
        var callbackInvocationCount = 0;

        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 1)
            .Add(p => p.ActiveTabIndexChanged, _ => callbackInvocationCount++)
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "B")));

        // Act
        renderedComponent.FindAll(".tab")[1].Click();

        // Assert
        callbackInvocationCount.Should().Be(0);
    }

    [Fact]
    public void Two_way_binding_should_update_active_tab_index()
    {
        // Arrange
        var receivedIndex = -1;

        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Bind(p => p.ActiveTabIndex, 0, value => receivedIndex = value)
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "B")));

        // Act
        renderedComponent.FindAll(".tab")[1].Click();

        // Assert
        renderedComponent.Instance.ActiveTabIndex.Should().Be(1);
        receivedIndex.Should().Be(1);
    }

    [Fact]
    public void Parameter_change_of_active_tab_index_should_be_reflected_on_instance()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .Add(p => p.ChildContent, BuildTabsFragment("A", "B")));

        // Act
        renderedComponent.Render(b => b
            .Add(p => p.ActiveTabIndex, 1)
            .Add(p => p.ChildContent, BuildTabsFragment("A", "B")));

        // Assert
        renderedComponent.Instance.ActiveTabIndex.Should().Be(1);
    }

    [Fact]
    public void Tab_click_should_invoke_scroll_to_active_tab_on_js()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "B")));

        // Act
        renderedComponent.FindAll(".tab")[1].Click();

        // Assert
        _jsAttachResult.VerifyInvoke("scrollToActiveTab");
    }

    [Fact]
    public void Tab_click_should_select_and_scroll_in_a_single_click()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "B"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "C")));

        // Act
        renderedComponent.FindAll(".tab")[2].Click();

        // Assert
        renderedComponent.Instance.ActiveTabIndex.Should().Be(2);
        renderedComponent.FindAll(".tab")[2].ClassList.Should().Contain("active");
        _jsAttachResult.VerifyInvoke("scrollToActiveTab");
    }

    [Theory]
    [InlineData("ArrowRight")]
    [InlineData("ArrowLeft")]
    public void Arrow_key_on_tab_should_not_change_active_tab_index(string key)
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 1)
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "B"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "C")));

        // Act
        renderedComponent.FindAll(".tab")[1].KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        renderedComponent.Instance.ActiveTabIndex.Should().Be(1);
    }

    [Theory]
    [InlineData("ArrowRight")]
    [InlineData("ArrowLeft")]
    public void Arrow_key_on_tab_should_not_invoke_active_tab_index_changed_callback(string key)
    {
        // Arrange
        var callbackInvocationCount = 0;

        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 1)
            .Add(p => p.ActiveTabIndexChanged, _ => callbackInvocationCount++)
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "B"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "C")));

        // Act
        renderedComponent.FindAll(".tab")[1].KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        callbackInvocationCount.Should().Be(0);
    }

    [Theory]
    [InlineData("Enter")]
    [InlineData("NumpadEnter")]
    public void Enter_key_on_tab_should_update_active_tab_index(string key)
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "B"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "C")));

        // Act
        renderedComponent.FindAll(".tab")[2].KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        renderedComponent.Instance.ActiveTabIndex.Should().Be(2);

        var tabs = renderedComponent.FindAll(".tab");
        tabs[0].ClassList.Should().NotContain("active");
        tabs[2].ClassList.Should().Contain("active");
    }

    [Theory]
    [InlineData("Enter")]
    [InlineData("NumpadEnter")]
    public void Enter_key_on_tab_should_invoke_active_tab_index_changed_callback(string key)
    {
        // Arrange
        int? receivedIndex = null;

        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .Add(p => p.ActiveTabIndexChanged, index => receivedIndex = index)
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "B")));

        // Act
        renderedComponent.FindAll(".tab")[1].KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        receivedIndex.Should().Be(1);
    }

    [Fact]
    public void Enter_key_on_tab_should_invoke_scroll_to_active_tab_on_js()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 0)
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "B")));

        // Act
        renderedComponent.FindAll(".tab")[1].KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        _jsAttachResult.VerifyInvoke("scrollToActiveTab");
    }

    [Fact]
    public void Enter_key_on_active_tab_should_not_invoke_active_tab_index_changed_callback()
    {
        // Arrange
        var callbackInvocationCount = 0;

        var renderedComponent = _testContext.Render<TabStripComponent>(b => b
            .Add(p => p.ActiveTabIndex, 1)
            .Add(p => p.ActiveTabIndexChanged, _ => callbackInvocationCount++)
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "A"))
            .AddChildContent<Tab>(t => t.Add(p => p.Text, "B")));

        // Act
        renderedComponent.FindAll(".tab")[1].KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        callbackInvocationCount.Should().Be(0);
    }

}
