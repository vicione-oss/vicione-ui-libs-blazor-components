using AwesomeAssertions;
using Bunit;
using ViciOne.Ui.Blazor.Components.Accordion.Components;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Accordion.Components;

public sealed partial class AccordionItemTests
{
    public sealed class OnBeforeExpand
    {
        [Fact]
        public void Should_be_invoked_with_sender()
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
        public void Cancel_should_prevent_expansion()
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
        public void Cancel_should_not_invoke_expanded_changed()
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
    }
}
