using Bunit;
using ViciOne.Ui.Blazor.Components.Popup.Components;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Extensions;
using PopupComponent = ViciOne.Ui.Blazor.Components.Popup.Components.Popup;

namespace ViciOne.Ui.Blazor.Components.Tests.Popup.Components;

public sealed partial class PopupTests
{
    public sealed class AttachJsAsync : IAsyncDisposable
    {
        private static readonly string s_jsModulePath =
            $"./_content/{typeof(PopupComponent).Assembly.GetName().Name}/popup/components/popup.js";

        private readonly BunitContext _testContext = new();

        public ValueTask DisposeAsync()
            => _testContext.DisposeAsync();

        [Fact]
        public async Task Should_dispose_module_without_creating_script_when_disposed_while_module_is_loading()
        {
            // Arrange
            _testContext.Services.AddPopup();
            _testContext.JSInterop.Mode = JSRuntimeMode.Loose;

            var pendingImport = PendingJSModuleImport.SetUp(_testContext, s_jsModulePath);

            // The module is loaded once a visible popup that closes on Escape has its modal dialog rendered.
            _testContext.Render<PopupRoot>();
            _testContext.Render<PopupComponent>(b => b
                .Add(p => p.CloseOnEscape, true)
                .Add(p => p.Visible, true));
            await pendingImport.WaitUntilRequestedAsync();

            // Act
            await _testContext.DisposeComponentsAsync();
            await pendingImport.CompleteAsync();

            // Assert
            await pendingImport.Module.Received(1).DisposeAsync();
            await pendingImport.Module.DidNotReceive()
                .InvokeConstructorAsync(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<object?[]?>());
        }
    }
}
