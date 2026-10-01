using Bunit;
using Microsoft.JSInterop;

using ButtonComponent = ViciOne.Ui.Blazor.Components.Button.Button;

namespace ViciOne.Ui.Blazor.Components.Tests.Button;

public sealed partial class ButtonTests
{
    public sealed class Ripple
    {
        private const string RippleModulePath = "./_content/ViciOne.Ui.Blazor.Components/material-web/ripple.js";

        [Fact]
        public void Should_load_ripple_module()
        {
            // Arrange
            using var testContext = new BunitContext();
            var import = testContext.JSInterop.SetupVoid("import", RippleModulePath);

            // Act
            testContext.Render<ButtonComponent>();

            // Assert
            import.Invocations.Should().ContainSingle();
        }

        [Fact]
        public void Should_load_ripple_module_once_for_all_buttons()
        {
            // Arrange
            using var testContext = new BunitContext();
            var import = testContext.JSInterop.SetupVoid("import", RippleModulePath);

            // Act
            testContext.Render<ButtonComponent>();
            testContext.Render<ButtonComponent>();

            // Assert
            import.Invocations.Should().ContainSingle();
        }

        [Fact]
        public void Should_render_when_ripple_module_fails_to_load()
        {
            // Arrange
            using var testContext = new BunitContext();
            testContext.JSInterop.SetupVoid("import", RippleModulePath).SetException(new JSException("Failed to fetch"));

            // Act
            var render = () => testContext.Render<ButtonComponent>();

            // Assert
            render.Should().NotThrow();
        }

        [Fact]
        public void Should_render_enabled_ripple()
        {
            // Arrange
            using var testContext = new BunitContext();

            // Act
            var renderedComponent = testContext.Render<ButtonComponent>();

            // Assert
            renderedComponent.Find("button > md-ripple").HasAttribute("disabled").Should().BeFalse();
        }

        [Fact]
        public void Should_disable_ripple_when_not_enabled()
        {
            // Arrange
            using var testContext = new BunitContext();

            // Act
            var renderedComponent = testContext.Render<ButtonComponent>(b => b
                .Add(p => p.Enabled, false));

            // Assert
            renderedComponent.Find("button > md-ripple").HasAttribute("disabled").Should().BeTrue();
        }

        [Fact]
        public void Should_disable_ripple_when_busy()
        {
            // Arrange
            using var testContext = new BunitContext();

            // Act
            var renderedComponent = testContext.Render<ButtonComponent>(b => b
                .Add(p => p.Busy, true));

            // Assert
            renderedComponent.Find("button > md-ripple").HasAttribute("disabled").Should().BeTrue();
        }

        [Fact]
        public void Should_enable_ripple_when_no_longer_busy()
        {
            // Arrange
            using var testContext = new BunitContext();

            var renderedComponent = testContext.Render<ButtonComponent>(b => b
                .Add(p => p.Busy, true));

            // Act
            renderedComponent.Render(b => b
                .Add(p => p.Busy, false));

            // Assert
            renderedComponent.Find("button > md-ripple").HasAttribute("disabled").Should().BeFalse();
        }
    }
}
