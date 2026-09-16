using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Button.Enums;
using ViciOne.Ui.Blazor.Components.Button.Extensions;
using ViciOne.Ui.Blazor.Components.Factories;

using ButtonComponent = ViciOne.Ui.Blazor.Components.Button.Button;

namespace ViciOne.Ui.Blazor.Components.Tests.Button;

public sealed partial class ButtonTests
{
    public sealed class Busy
    {
        public static readonly TheoryData<string> BusyIndications =
            [.. TypeSafeEnumFactory<ButtonBusyIndication>.CreateAll().Select(busyIndication => busyIndication.GetName())];

        [Fact]
        public void Should_render_busy_modifier_css_class()
        {
            // Arrange
            using var testContext = new BunitContext();

            // Act
            var renderedComponent = testContext.Render<ButtonComponent>(b => b
                .Add(p => p.Busy, true));

            // Assert
            renderedComponent.Find("button").ClassList.Should().Contain("button--busy");
        }

        [Fact]
        public void Should_render_no_busy_modifier_css_class_when_not_busy()
        {
            // Arrange
            using var testContext = new BunitContext();

            // Act
            var renderedComponent = testContext.Render<ButtonComponent>();

            // Assert
            renderedComponent.Find("button").ClassList.Should().NotContain("button--busy");
        }

        [Fact]
        public void Should_report_busy_and_disabled()
        {
            // Arrange
            using var testContext = new BunitContext();

            // Act
            var renderedComponent = testContext.Render<ButtonComponent>(b => b
                .Add(p => p.Busy, true));

            // Assert
            var button = renderedComponent.Find("button");
            button.GetAttribute("aria-busy").Should().Be("true");
            button.GetAttribute("aria-disabled").Should().Be("true");
        }

        [Fact]
        public void Should_report_neither_busy_nor_disabled_when_not_busy()
        {
            // Arrange
            using var testContext = new BunitContext();

            // Act
            var renderedComponent = testContext.Render<ButtonComponent>();

            // Assert
            var button = renderedComponent.Find("button");
            button.HasAttribute("aria-busy").Should().BeFalse();
            button.HasAttribute("aria-disabled").Should().BeFalse();
        }

        [Fact]
        public void Should_render_no_disabled_attribute()
        {
            // Arrange
            using var testContext = new BunitContext();

            // Act
            var renderedComponent = testContext.Render<ButtonComponent>(b => b
                .Add(p => p.Busy, true));

            // Assert
            renderedComponent.Find("button").HasAttribute("disabled").Should().BeFalse();
        }

        [Fact]
        public void Should_render_sweep_modifier_css_class_by_default()
        {
            // Arrange
            using var testContext = new BunitContext();

            // Act
            var renderedComponent = testContext.Render<ButtonComponent>(b => b
                .Add(p => p.Busy, true));

            // Assert
            var cssClasses = renderedComponent.Find("button").ClassList;
            cssClasses.Should().Contain("button--busy-sweep");
            cssClasses.Should().NotContain("button--busy-spinning-icon");
        }

        [Theory]
        [MemberData(nameof(BusyIndications))]
        public void Should_render_busy_indication_modifier_css_classes(string busyIndication)
        {
            // Arrange
            using var testContext = new BunitContext();

            var busyIndicationTyped = TypeSafeEnumFactory<ButtonBusyIndication>.Create(busyIndication);
            var modifierCssClasses = busyIndicationTyped.ToModifierCssClasses();

            // Act
            var renderedComponent = testContext.Render<ButtonComponent>(b => b
                .Add(p => p.Busy, true)
                .Add(p => p.BusyIndication, busyIndicationTyped));

            // Assert
            renderedComponent.Find("button").ClassList.Should().Contain(modifierCssClasses);
        }

        [Fact]
        public void Should_render_both_modifier_css_classes_for_sweep_and_spinning_icon()
        {
            // Arrange
            using var testContext = new BunitContext();

            // Act
            var renderedComponent = testContext.Render<ButtonComponent>(b => b
                .Add(p => p.Busy, true)
                .Add(p => p.BusyIndication, ButtonBusyIndication.SweepAndSpinningIcon));

            // Assert
            var cssClasses = renderedComponent.Find("button").ClassList;
            cssClasses.Should().Contain("button--busy-sweep");
            cssClasses.Should().Contain("button--busy-spinning-icon");
            cssClasses.Should().NotContain("button--busy-sweep-and-spinning-icon");
        }

        [Fact]
        public void Should_render_default_indication_modifier_css_class_for_uninitialized_indication()
        {
            // Arrange
            using var testContext = new BunitContext();
            ButtonBusyIndication uninitializedBusyIndication = default;

            // Act
            var renderedComponent = testContext.Render<ButtonComponent>(b => b
                .Add(p => p.Busy, true)
                .Add(p => p.BusyIndication, uninitializedBusyIndication));

            // Assert
            renderedComponent.Find("button").ClassList.Should().Contain("button--busy-sweep");
        }

        [Fact]
        public void Should_render_no_busy_modifier_css_class_when_not_enabled()
        {
            // Arrange
            using var testContext = new BunitContext();

            // Act
            var renderedComponent = testContext.Render<ButtonComponent>(b => b
                .Add(p => p.Busy, true)
                .Add(p => p.Enabled, false));

            // Assert
            var cssClasses = renderedComponent.Find("button").ClassList;
            cssClasses.Should().NotContain("button--busy");
            cssClasses.Should().NotContain("button--busy-sweep");
        }

        [Fact]
        public void Should_report_neither_busy_nor_disabled_when_not_enabled()
        {
            // Arrange
            using var testContext = new BunitContext();

            // Act
            var renderedComponent = testContext.Render<ButtonComponent>(b => b
                .Add(p => p.Busy, true)
                .Add(p => p.Enabled, false));

            // Assert
            var button = renderedComponent.Find("button");
            button.HasAttribute("aria-busy").Should().BeFalse();
            button.HasAttribute("aria-disabled").Should().BeFalse();
            button.HasAttribute("disabled").Should().BeTrue();
        }

        [Fact]
        public void Should_not_raise_on_click()
        {
            // Arrange
            using var testContext = new BunitContext();
            var clickCount = 0;

            var renderedComponent = testContext.Render<ButtonComponent>(b => b
                .Add(p => p.Busy, true)
                .Add(p => p.OnClick, EventCallback.Factory.Create(this, () => clickCount++)));

            // Act
            var click = () => renderedComponent.Find("button").Click();

            // Assert
            click.Should().Throw<MissingEventHandlerException>();
            clickCount.Should().Be(0);
        }
    }
}
