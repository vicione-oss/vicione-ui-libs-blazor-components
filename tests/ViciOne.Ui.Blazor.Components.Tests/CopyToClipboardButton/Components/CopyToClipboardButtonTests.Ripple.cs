using Bunit;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.TestingHelpers.CopyToClipboardButton.Extensions;

using ButtonComponent = ViciOne.Ui.Blazor.Components.Button.Button;
using CopyToClipboardButtonComponent = ViciOne.Ui.Blazor.Components.CopyToClipboardButton.Components.CopyToClipboardButton;

namespace ViciOne.Ui.Blazor.Components.Tests.CopyToClipboardButton.Components;

public sealed partial class CopyToClipboardButtonTests
{
    public sealed class Ripple
    {
        private const string RippleModulePath = "./_content/ViciOne.Ui.Blazor.Components/material-web/ripple.js";

        [Fact]
        public void Should_load_ripple_module()
        {
            // Arrange
            using var testContext = new BunitContext();
            testContext.JSInterop.SetupForCopyToClipboardButton();
            var import = testContext.JSInterop.SetupVoid("import", RippleModulePath);

            // Act
            RenderCopyToClipboardButton(testContext);

            // Assert
            import.Invocations.Should().ContainSingle();
        }

        [Fact]
        public void Should_load_ripple_module_once_for_all_copy_to_clipboard_buttons()
        {
            // Arrange
            using var testContext = new BunitContext();
            testContext.JSInterop.SetupForCopyToClipboardButton();
            var import = testContext.JSInterop.SetupVoid("import", RippleModulePath);

            // Act
            RenderCopyToClipboardButton(testContext);
            RenderCopyToClipboardButton(testContext);

            // Assert
            import.Invocations.Should().ContainSingle();
        }

        [Fact]
        public void Should_load_ripple_module_once_together_with_buttons()
        {
            // Arrange
            using var testContext = new BunitContext();
            testContext.JSInterop.SetupForCopyToClipboardButton();
            var import = testContext.JSInterop.SetupVoid("import", RippleModulePath);

            // Act
            testContext.Render<ButtonComponent>();
            RenderCopyToClipboardButton(testContext);

            // Assert
            import.Invocations.Should().ContainSingle();
        }

        [Fact]
        public void Should_render_when_ripple_module_fails_to_load()
        {
            // Arrange
            using var testContext = new BunitContext();
            testContext.JSInterop.SetupForCopyToClipboardButton();
            testContext.JSInterop.SetupVoid("import", RippleModulePath).SetException(new JSException("Failed to fetch"));

            // Act
            var render = () => RenderCopyToClipboardButton(testContext);

            // Assert
            render.Should().NotThrow();
        }

        [Fact]
        public void Should_render_enabled_ripple()
        {
            // Arrange
            using var testContext = new BunitContext();
            testContext.JSInterop.SetupForCopyToClipboardButton();

            // Act
            var renderedComponent = RenderCopyToClipboardButton(testContext);

            // Assert
            renderedComponent.Find("button > md-ripple").HasAttribute("disabled").Should().BeFalse();
        }

        [Fact]
        public void Should_disable_ripple_when_not_enabled()
        {
            // Arrange
            using var testContext = new BunitContext();
            testContext.JSInterop.SetupForCopyToClipboardButton();

            // Act
            var renderedComponent = testContext.Render<CopyToClipboardButtonComponent>(b => b
                .Add(p => p.TextToCopy, () => Task.FromResult("text"))
                .Add(p => p.Enabled, false));

            // Assert
            renderedComponent.Find("button > md-ripple").HasAttribute("disabled").Should().BeTrue();
        }

        [Fact]
        public void Should_enable_ripple_when_enabled_again()
        {
            // Arrange
            using var testContext = new BunitContext();
            testContext.JSInterop.SetupForCopyToClipboardButton();

            var renderedComponent = testContext.Render<CopyToClipboardButtonComponent>(b => b
                .Add(p => p.TextToCopy, () => Task.FromResult("text"))
                .Add(p => p.Enabled, false));

            // Act
            renderedComponent.Render(b => b
                .Add(p => p.Enabled, true));

            // Assert
            renderedComponent.Find("button > md-ripple").HasAttribute("disabled").Should().BeFalse();
        }

        private static IRenderedComponent<CopyToClipboardButtonComponent> RenderCopyToClipboardButton(BunitContext testContext)
            => testContext.Render<CopyToClipboardButtonComponent>(b => b
                .Add(p => p.TextToCopy, () => Task.FromResult("text")));
    }
}
