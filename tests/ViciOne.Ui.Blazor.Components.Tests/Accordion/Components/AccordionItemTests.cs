using AwesomeAssertions;
using Bunit;
using ViciOne.Ui.Blazor.Components.Accordion.Components;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Accordion.Components;

public sealed class AccordionItemTests
{
    [Fact]
    public void Should_render_root_element_with_accordion_item_class()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<AccordionItem>(b =>
            b.Add(p => p.Text, "Item 1"));

        // Assert
        renderedComponent.Find(".accordion-item");
    }

    [Fact]
    public void Should_render_header_with_text()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<AccordionItem>(b =>
            b.Add(p => p.Text, "Network"));

        // Assert
        var text = renderedComponent.Find(".header .text");
        text.TextContent.Should().Be("Network");
    }

    /// <summary>
    /// https://www.w3.org/WAI/ARIA/apg/patterns/button/examples/button/#kbd_label
    /// </summary>
    [Fact]
    public void Should_render_header_as_button_for_keyboard_accessibility()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<AccordionItem>(b =>
            b.Add(p => p.Text, "Item"));

        var header = renderedComponent.Find("button.header");

        // Assert
        header.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("custom-class")]
    [InlineData("first second")]
    public void Should_render_css_class(string? cssClass)
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");

            if (cssClass is not null)
                b.Add(p => p.CssClass, cssClass);
        });

        var root = renderedComponent.Find(".accordion-item");
        var classes = root.GetAttribute("class")!;

        // Assert
        classes.Should().Contain("accordion-item");

        if (cssClass is not null)
            classes.Should().Contain(cssClass);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Settings panel")]
    public void Should_render_title_attribute(string? title)
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");

            if (title is not null)
                b.Add(p => p.Title, title);
        });

        var root = renderedComponent.Find(".accordion-item");

        // Assert
        if (title is not null)
            root.GetAttribute("title").Should().Be(title);
        else
            root.GetAttribute("title").Should().BeNull();
    }

    [Fact]
    public void Should_not_render_content_when_collapsed()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");
            b.Add(p => p.ChildContent, "<div class=\"inner\">Content</div>");
        });

        // Assert
        renderedComponent.FindAll(".content").Should().BeEmpty();
    }

    [Fact]
    public void Should_render_content_when_expanded()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");
            b.Add(p => p.Expanded, true);
            b.Add(p => p.ChildContent, "<div class=\"inner\">Content</div>");
        });

        // Assert
        var content = renderedComponent.Find(".content .inner");
        content.TextContent.Should().Be("Content");
    }

    [Fact]
    public void Should_not_render_content_div_when_expanded_but_no_child_content()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");
            b.Add(p => p.Expanded, true);
        });

        // Assert
        renderedComponent.FindAll(".content").Should().BeEmpty();
    }

    [Fact]
    public void Click_on_header_should_expand_collapsed_item()
    {
        // Arrange
        using var testContext = new BunitContext();

        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");
            b.Add(p => p.ChildContent, "<div class=\"inner\">Content</div>");
        });

        // Act
        renderedComponent.Find(".header").Click();

        // Assert
        renderedComponent.Find(".content .inner");
        renderedComponent.Instance.Expanded.Should().BeTrue();
    }

    [Fact]
    public void Click_on_header_should_collapse_expanded_item()
    {
        // Arrange
        using var testContext = new BunitContext();

        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");
            b.Add(p => p.Expanded, true);
            b.Add(p => p.ChildContent, "<div class=\"inner\">Content</div>");
        });

        // Act
        renderedComponent.Find(".header").Click();

        // Assert
        renderedComponent.FindAll(".content").Should().BeEmpty();
        renderedComponent.Instance.Expanded.Should().BeFalse();
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Click_on_header_should_invoke_expanded_changed(bool initialExpanded, bool expectedExpanded)
    {
        // Arrange
        using var testContext = new BunitContext();
        bool? receivedValue = null;

        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");
            b.Add(p => p.Expanded, initialExpanded);
            b.Add(p => p.ExpandedChanged, value => receivedValue = value);
            b.Add(p => p.ChildContent, "<div>Content</div>");
        });

        // Act
        renderedComponent.Find(".header").Click();

        // Assert
        receivedValue.Should().Be(expectedExpanded);
    }

    [Fact]
    public void OnBeforeExpand_should_be_invoked_with_sender()
    {
        // Arrange
        using var testContext = new BunitContext();
        IAccordionItem? receivedSender = null;

        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Network");
            b.Add(p => p.OnBeforeExpand, args => receivedSender = args.Sender);
            b.Add(p => p.ChildContent, "<div>Content</div>");
        });

        // Act
        renderedComponent.Find(".header").Click();

        // Assert
        receivedSender.Should().NotBeNull();
        receivedSender!.Text.Should().Be("Network");
    }

    [Fact]
    public void OnBeforeExpand_cancel_should_prevent_expansion()
    {
        // Arrange
        using var testContext = new BunitContext();

        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");
            b.Add(p => p.OnBeforeExpand, args => args.Cancel = true);
            b.Add(p => p.ChildContent, "<div>Content</div>");
        });

        // Act
        renderedComponent.Find(".header").Click();

        // Assert
        renderedComponent.FindAll(".content").Should().BeEmpty();
        renderedComponent.Instance.Expanded.Should().BeFalse();
    }

    [Fact]
    public void OnBeforeExpand_cancel_should_not_invoke_expanded_changed()
    {
        // Arrange
        using var testContext = new BunitContext();
        var expandedChangedInvoked = false;

        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");
            b.Add(p => p.OnBeforeExpand, args => args.Cancel = true);
            b.Add(p => p.ExpandedChanged, _ => expandedChangedInvoked = true);
            b.Add(p => p.ChildContent, "<div>Content</div>");
        });

        // Act
        renderedComponent.Find(".header").Click();

        // Assert
        expandedChangedInvoked.Should().BeFalse();
    }

    [Fact]
    public void OnBeforeCollapse_should_be_invoked_with_sender()
    {
        // Arrange
        using var testContext = new BunitContext();
        IAccordionItem? receivedSender = null;

        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Settings");
            b.Add(p => p.Expanded, true);
            b.Add(p => p.OnBeforeCollapse, args => receivedSender = args.Sender);
            b.Add(p => p.ChildContent, "<div>Content</div>");
        });

        // Act
        renderedComponent.Find(".header").Click();

        // Assert
        receivedSender.Should().NotBeNull();
        receivedSender!.Text.Should().Be("Settings");
    }

    [Fact]
    public void OnBeforeCollapse_cancel_should_prevent_collapse()
    {
        // Arrange
        using var testContext = new BunitContext();

        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");
            b.Add(p => p.Expanded, true);
            b.Add(p => p.OnBeforeCollapse, args => args.Cancel = true);
            b.Add(p => p.ChildContent, "<div class=\"inner\">Content</div>");
        });

        // Act
        renderedComponent.Find(".header").Click();

        // Assert
        renderedComponent.Find(".content .inner");
        renderedComponent.Instance.Expanded.Should().BeTrue();
    }

    [Fact]
    public void OnBeforeCollapse_cancel_should_not_invoke_expanded_changed()
    {
        // Arrange
        using var testContext = new BunitContext();
        var expandedChangedInvoked = false;

        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");
            b.Add(p => p.Expanded, true);
            b.Add(p => p.OnBeforeCollapse, args => args.Cancel = true);
            b.Add(p => p.ExpandedChanged, _ => expandedChangedInvoked = true);
            b.Add(p => p.ChildContent, "<div>Content</div>");
        });

        // Act
        renderedComponent.Find(".header").Click();

        // Assert
        expandedChangedInvoked.Should().BeFalse();
    }

    [Fact]
    public void Should_render_icon_from_css_class()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");
            b.Add(p => p.IconCssClass, "my-icon");
        });

        // Assert
        var icon = renderedComponent.Find(".icon");
        icon.ClassList.Should().Contain("my-icon");
    }

    [Fact]
    public void Should_render_icon_from_url()
    {
        // Arrange
        using var testContext = new BunitContext();
        var iconUri = new Uri("https://example.com/icon.png");

        // Act
        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");
            b.Add(p => p.IconUrl, iconUri);
        });

        // Assert
        var icon = renderedComponent.Find("img.icon");
        icon.GetAttribute("src").Should().Be(iconUri.ToString());
    }

    [Fact]
    public void Should_render_icon_from_data_uri()
    {
        // Arrange
        using var testContext = new BunitContext();
        const string IconData = "data:image/png;base64,abc123";

        // Act
        var renderedComponent = testContext.Render<AccordionItem>(b =>
        {
            b.Add(p => p.Text, "Item");
            b.Add(p => p.IconData, IconData);
        });

        // Assert
        var icon = renderedComponent.Find("img.icon");
        icon.GetAttribute("src").Should().Be(IconData);
    }

    [Fact]
    public void Should_not_render_icon_when_none_specified()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<AccordionItem>(b =>
            b.Add(p => p.Text, "Item"));

        // Assert
        renderedComponent.FindAll(".icon").Should().BeEmpty();
    }
}
