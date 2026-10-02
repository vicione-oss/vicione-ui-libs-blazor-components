using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Moveable.Components;
using ViciOne.Ui.Blazor.Components.Popup.Components;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Popup.Extensions;
using PopupComponent = ViciOne.Ui.Blazor.Components.Popup.Components.Popup;

namespace ViciOne.Ui.Blazor.Components.Tests.Popup.Components;

public sealed partial class PopupTests
{
    // Escape is listened to by the JS instance of the popup, which bUnit does not run. These tests cover
    // attaching and removing that listener; the key press itself is covered by the Server tests.
    public sealed class CloseOnEscape : IAsyncDisposable
    {
        private static readonly string s_jsModulePath =
            $"./_content/{typeof(PopupComponent).Assembly.GetName().Name}/popup/components/popup.js";

        private readonly BunitContext _testContext = new();
        private readonly IRenderedComponent<PopupRoot> _popupRoot;
        private readonly BunitJSModuleInterop _jsModule;
        private readonly BunitJSModuleInterop _jsInstance;

        public CloseOnEscape()
        {
            _testContext.Services.AddPopup();
            _testContext.JSInterop.SetupForPopup();

            // Replaces the loose module set up above, so these tests can tell the calls to it apart.
            _jsModule = _testContext.JSInterop.SetupModule(s_jsModulePath);

            _jsInstance = _jsModule.SetupModule("Popup", _ => true);
            _jsInstance.SetupVoid("dispose").SetVoidResult();

            _popupRoot = _testContext.Render<PopupRoot>();
        }

        public ValueTask DisposeAsync()
        {
            _popupRoot.Dispose();

            return _testContext.DisposeAsync();
        }

        [Fact]
        public void Should_listen_for_escape_on_modal_dialog_when_enabled()
        {
            // Act
            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.CloseOnEscape, true)
                .Add(p => p.Visible, true));

            // Assert
            // The moveable element of the popup is its modal dialog.
            var modalDialogElementReference = ((IMoveable)popup.Instance).GetElementReference();

            var invocation = _jsModule.Invocations.Should().ContainSingle(i => i.Identifier == "Popup").Subject;
            ((ElementReference)invocation.Arguments[1]!).Id.Should().Be(modalDialogElementReference.Id);
        }

        [Fact]
        public void Should_not_listen_for_escape_when_disabled()
        {
            // Act
            _testContext.Render<PopupComponent>(b => b
                .Add(p => p.CssClass, "test-popup")
                .Add(p => p.CloseOnEscape, false)
                .Add(p => p.Visible, true));

            // Assert
            // Every key listened to would otherwise cost a call into JS, so a popup that does not close on
            // Escape does not load the module at all.
            _testContext.JSInterop.Invocations.Should().NotContain(i =>
                i.Identifier == "import" && Equals(i.Arguments[0], s_jsModulePath));
        }

        [Fact]
        public void Should_not_listen_for_escape_while_hidden()
        {
            // Act
            _testContext.Render<PopupComponent>(b => b
                .Add(p => p.CloseOnEscape, true)
                .Add(p => p.Visible, false));

            // Assert
            _jsModule.Invocations.Should().NotContain(i => i.Identifier == "Popup");
        }

        [Fact]
        public async Task Should_stop_listening_for_escape_when_closed()
        {
            // Arrange
            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.CloseOnEscape, true)
                .Add(p => p.Visible, true));

            // Act
            await popup.Instance.CloseAsync();

            // Assert
            _jsInstance.VerifyInvoke("dispose");
        }

        [Fact]
        public void Should_stop_listening_for_escape_when_disabled_while_visible()
        {
            // Arrange
            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.CloseOnEscape, true)
                .Add(p => p.Visible, true));

            // Act
            popup.Render(b => b
                .Add(p => p.CloseOnEscape, false));

            // Assert
            _jsInstance.VerifyInvoke("dispose");
        }

        [Fact]
        public async Task Should_listen_for_escape_again_when_shown_again()
        {
            // Arrange
            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.CloseOnEscape, true)
                .Add(p => p.Visible, true));

            await popup.Instance.CloseAsync();

            // Act
            // The dialog element is created anew when shown, so the listener has to be attached to that one.
            await popup.Instance.ShowAsync();

            // Assert
            _jsModule.Invocations.Count(i => i.Identifier == "Popup").Should().Be(2);
        }
    }
}
