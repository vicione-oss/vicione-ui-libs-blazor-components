using Bunit;
using ViciOne.Ui.Blazor.Components.Accordion.Components;

namespace ViciOne.Ui.Blazor.Components.Tests.Accordion.Components;

public sealed partial class AccordionItemTests
{
    public sealed class OnBeforeCollapse
    {
        [Fact]
        public void Should_be_invoked_with_sender()
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
        public void Cancel_should_prevent_collapse()
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
        public void Cancel_should_not_invoke_expanded_changed()
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
    }
}
