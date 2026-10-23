using Bunit;
using ViciOne.Ui.Blazor.Components.Accordion.Components;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Accordion.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Accordion.Components;

public sealed partial class AccordionItemTests
{
    public sealed class ScrollIntoViewAsync
    {
        [Fact]
        public async Task Should_scroll_root_element_into_view()
        {
            // Arrange
            await using var testContext = new BunitContext();

            var jsModule = testContext.JSInterop.SetupForAccordionItem();

            var renderedComponent = testContext.Render<AccordionItem>(b => b
                .Add(p => p.Text, "Network"));

            // Act
            await renderedComponent.InvokeAsync(renderedComponent.Instance.ScrollIntoViewAsync);

            // Assert
            jsModule.VerifyInvoke("scrollIntoView").Arguments[0].ShouldBeElementReferenceTo(renderedComponent.Find(".accordion-item"));
        }

        [Fact]
        public async Task Should_end_pending_scroll_on_disposal()
        {
            // Arrange
            await using var testContext = new BunitContext();

            // A planned invocation without a result stays pending until it is canceled.
            var jsModule = testContext.JSInterop.SetupForAccordionItem();
            jsModule.SetupVoid("scrollIntoView", _ => true);

            var renderedComponent = testContext.Render<AccordionItem>(b => b
                .Add(p => p.Text, "Network"));

            var scrollTask = renderedComponent.Instance.ScrollIntoViewAsync();

            // Act
            await testContext.DisposeComponentsAsync();

            // Assert
            await scrollTask.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
        }

        [Fact]
        public async Task Should_not_import_module_after_disposal()
        {
            // Arrange
            await using var testContext = new BunitContext();

            testContext.JSInterop.SetupForAccordionItem();

            var renderedComponent = testContext.Render<AccordionItem>(b => b
                .Add(p => p.Text, "Network"));

            var accordionItem = renderedComponent.Instance;
            await testContext.DisposeComponentsAsync();

            // Act
            await accordionItem.ScrollIntoViewAsync();

            // Assert
            testContext.JSInterop.Invocations.Should().BeEmpty();
        }
    }
}
