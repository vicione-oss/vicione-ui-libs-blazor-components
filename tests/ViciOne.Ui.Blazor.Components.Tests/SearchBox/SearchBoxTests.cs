using Bunit;
using Microsoft.AspNetCore.Components.Web;
using SearchBoxComponent = ViciOne.Ui.Blazor.Components.SearchBox.SearchBox;

namespace ViciOne.Ui.Blazor.Components.Tests.SearchBox;

public sealed class SearchBoxTests : IDisposable
{
    private readonly BunitContext _testContext;

    public SearchBoxTests()
    {
        _testContext = new BunitContext();

        // TextBox imports a JS module; set up a catch-all so bUnit doesn't throw
        _testContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public void Dispose()
        => _testContext.Dispose();

    [Fact]
    public void Should_render_without_parameters()
    {
        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>();

        // Assert
        renderedComponent.Find(".search-box").Should().NotBeNull();
    }

    [Fact]
    public void Should_render_placeholder()
    {
        // Arrange
        const string Placeholder = "Search here...";

        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.Placeholder, Placeholder));

        // Assert
        var input = renderedComponent.Find("input");
        input.GetAttribute("placeholder").Should().Be(Placeholder);
    }

    [Fact]
    public void Should_apply_custom_css_class()
    {
        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.CssClass, "my-custom-class"));

        // Assert
        var container = renderedComponent.Find(".search-box");
        container.GetAttribute("class").Should().Contain("my-custom-class");
    }

    [Fact]
    public void Should_render_input_with_search_text_box_css_class()
    {
        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>();

        // Assert
        var input = renderedComponent.Find("input");
        input.GetAttribute("class").Should().Contain("search-box-input");
    }

    [Fact]
    public void Should_render_disabled_input_when_not_enabled()
    {
        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.Enabled, false));

        // Assert
        var input = renderedComponent.Find("input");
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Should_hide_clear_button_when_text_is_empty()
    {
        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>();

        // Assert
        var clearButton = renderedComponent.Find("button");
        clearButton.GetAttribute("class").Should().Contain("hidden");
    }

    [Fact]
    public void Should_show_clear_button_when_text_is_set()
    {
        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.Text, "hello"));

        // Assert
        var clearButton = renderedComponent.Find("button");
        clearButton.GetAttribute("class").Should().NotContain("hidden");
    }

    [Fact]
    public void Should_disable_clear_button_when_not_enabled_even_with_text()
    {
        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.Text, "hello")
            .Add(p => p.Enabled, false));

        // Assert
        var clearButton = renderedComponent.Find("button");
        clearButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Should_hide_icon_container_when_text_is_entered()
    {
        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.Text, "hello"));

        // Assert
        var iconContainer = renderedComponent.Find(".icon-container");
        iconContainer.GetAttribute("class").Should().Contain("hidden");
    }

    [Fact]
    public void Should_show_icon_container_when_text_is_empty()
    {
        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>();

        // Assert
        var iconContainer = renderedComponent.Find(".icon-container");
        iconContainer.GetAttribute("class").Should().NotContain("hidden");
    }

    [Fact]
    public void Should_add_disabled_class_to_icon_container_when_not_enabled()
    {
        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.Enabled, false));

        // Assert
        var iconContainer = renderedComponent.Find(".icon-container");
        iconContainer.GetAttribute("class").Should().Contain("disabled");
    }

    [Fact]
    public void Should_invoke_text_changing_on_input()
    {
        // Arrange
        string? receivedValue = null;
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.TextChanging, v => receivedValue = v));

        // Act
        var input = renderedComponent.Find("input");
        input.Input("test");

        // Assert
        receivedValue.Should().Be("test");
    }

    [Fact]
    public void Should_invoke_text_changed_on_blur()
    {
        // Arrange
        string? receivedValue = null;
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.TextChanged, v => receivedValue = v));

        // Act
        var input = renderedComponent.Find("input");
        input.Input("test");
        input.Blur();

        // Assert
        receivedValue.Should().Be("test");
    }

    [Fact]
    public void Should_invoke_enter_pressed_with_the_entered_text()
    {
        // Arrange
        string? receivedValue = null;
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.EnterPressed, v => receivedValue = v));

        // Act
        var input = renderedComponent.Find("input");
        input.Input("test");
        input.KeyUp(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        receivedValue.Should().Be("test");
    }

    [Fact]
    public void Should_not_invoke_enter_pressed_for_other_keys()
    {
        // Arrange
        var invocationCount = 0;
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.EnterPressed, _ => invocationCount++));

        // Act
        var input = renderedComponent.Find("input");
        input.Input("test");
        input.KeyUp(new KeyboardEventArgs { Key = "a" });

        // Assert
        invocationCount.Should().Be(0);
    }

    [Fact]
    public void Should_invoke_text_changing_and_text_changed_on_clear_button_click()
    {
        // Arrange
        var changingValue = (string?)"initial";
        var changedValue = (string?)"initial";
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.Text, "hello")
            .Add(p => p.TextChanging, v => changingValue = v)
            .Add(p => p.TextChanged, v => changedValue = v));

        // Act
        var clearButton = renderedComponent.Find("button");
        clearButton.Click();

        // Assert
        changingValue.Should().BeNull();
        changedValue.Should().BeNull();
    }

    [Fact]
    public void Should_hide_clear_button_after_clearing()
    {
        // Arrange
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.Text, "hello"));

        // Precondition
        renderedComponent.Find("button").GetAttribute("class").Should().NotContain("hidden");

        // Act
        renderedComponent.Find("button").Click();

        // Assert
        renderedComponent.Find("button").GetAttribute("class").Should().Contain("hidden");
    }

    [Fact]
    public void Should_show_icon_container_after_clearing()
    {
        // Arrange
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.Text, "hello"));

        // Precondition
        renderedComponent.Find(".icon-container").GetAttribute("class").Should().Contain("hidden");

        // Act
        renderedComponent.Find("button").Click();

        // Assert
        renderedComponent.Find(".icon-container").GetAttribute("class").Should().NotContain("hidden");
    }

    [Fact]
    public void Should_apply_animate_transition_class_after_input_changes()
    {
        // Arrange
        var renderedComponent = _testContext.Render<SearchBoxComponent>();

        // Act – type into the input to trigger animation flag
        renderedComponent.Find("input").Input("x");

        // Assert
        var clearButton = renderedComponent.Find("button");
        clearButton.GetAttribute("class").Should().Contain("animate-transition");

        var iconContainer = renderedComponent.Find(".icon-container");
        iconContainer.GetAttribute("class").Should().Contain("animate-transition");
    }

    [Fact]
    public void Should_set_tabindex_negative_one_on_hidden_clear_button()
    {
        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>();

        // Assert
        var clearButton = renderedComponent.Find("button");
        clearButton.GetAttribute("tabindex").Should().Be("-1");
    }

    [Fact]
    public void Should_not_set_tabindex_on_visible_clear_button()
    {
        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.Text, "hello"));

        // Assert
        var clearButton = renderedComponent.Find("button");
        clearButton.HasAttribute("tabindex").Should().BeFalse();
    }

    [Fact]
    public void Should_render_default_search_icon_when_no_icon_parameters_set()
    {
        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>();

        // Assert – the icon-container should contain the default MonochromeIcon (svg element)
        var iconContainer = renderedComponent.Find(".icon-container");
        iconContainer.InnerHtml.Should().NotBeEmpty();
    }

    [Fact]
    public void Should_render_custom_icon_when_icon_css_class_is_set()
    {
        // Arrange
        const string CustomIconCss = "my-custom-icon";

        // Act
        var renderedComponent = _testContext.Render<SearchBoxComponent>(b => b
            .Add(p => p.IconCssClass, CustomIconCss));

        // Assert
        var iconContainer = renderedComponent.Find(".icon-container");
        iconContainer.InnerHtml.Should().Contain(CustomIconCss);
    }
}
