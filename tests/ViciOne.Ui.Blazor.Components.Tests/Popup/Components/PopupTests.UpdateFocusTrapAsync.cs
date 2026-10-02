using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Popup.Extensions;
using PopupComponent = ViciOne.Ui.Blazor.Components.Popup.Components.Popup;

namespace ViciOne.Ui.Blazor.Components.Tests.Popup.Components;

public sealed partial class PopupTests
{
    public sealed class UpdateFocusTrapAsync : IAsyncDisposable
    {
        private readonly BunitContext _testContext = new();

        public UpdateFocusTrapAsync()
        {
            _testContext.Services.AddPopup();
            _testContext.JSInterop.SetupForPopup();
        }

        public ValueTask DisposeAsync()
            => _testContext.DisposeAsync();

        [Fact]
        public void Should_trap_focus_in_dialog_when_shown()
        {
            // Act
            ShowPopup();

            // Assert
            GetAttachedElementIds().Should().ContainSingle()
                .Which.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Should_trap_focus_before_focusing_dialog()
        {
            // Act
            ShowPopup();

            // Assert
            // A popup below would otherwise pull the focus back, as it is not yet aware of being covered.
            var identifiers = _testContext.JSInterop.Invocations.Select(i => i.Identifier).ToList();

            identifiers.IndexOf("attach").Should().BeGreaterThanOrEqualTo(0)
                .And.BeLessThan(identifiers.IndexOf("Blazor._internal.domWrapper.focus"));
        }

        [Fact]
        public async Task Should_trap_focus_in_dialog_of_second_show()
        {
            // Arrange
            var popup = ShowPopup();

            await popup.Instance.CloseAsync();

            // Act
            // The closed dialog is gone, so the second show renders a new one the trap has to follow.
            await popup.Instance.ShowAsync();

            popup.RenderSectionContent(_testContext);

            popup.Render(b => b
                .Add(p => p.ShowBackdrop, true)
                .Add(p => p.Visible, true));

            // Assert
            popup.WaitForAssertion(() => GetAttachedElementIds()
                .Should().HaveCount(2).And.OnlyHaveUniqueItems());
        }

        [Fact]
        public async Task Should_release_focus_trap_on_close()
        {
            // Arrange
            var popup = ShowPopup();

            // Act
            await popup.Instance.CloseAsync();

            // Assert
            popup.WaitForAssertion(() => _testContext.JSInterop.Invocations
                .Should().Contain(i => i.Identifier == "dispose"));
        }

        [Fact]
        public async Task Should_release_focus_trap_on_dispose()
        {
            // Arrange
            var popup = ShowPopup();

            // Act
            await popup.Instance.DisposeAsync();

            // Assert
            _testContext.JSInterop.Invocations.Should().Contain(i => i.Identifier == "dispose");
        }

        private List<string> GetAttachedElementIds()
            => [.. _testContext.JSInterop.Invocations
                .Where(i => i.Identifier == "attach")
                .SelectMany(i => i.Arguments.OfType<ElementReference>())
                .Select(e => e.Id)];

        private IRenderedComponent<PopupComponent> ShowPopup()
        {
            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.ShowBackdrop, true)
                .Add(p => p.Visible, true));

            popup.RenderSectionContent(_testContext);

            popup.Render(b => b
                .Add(p => p.ShowBackdrop, true)
                .Add(p => p.Visible, true));

            return popup;
        }
    }
}
