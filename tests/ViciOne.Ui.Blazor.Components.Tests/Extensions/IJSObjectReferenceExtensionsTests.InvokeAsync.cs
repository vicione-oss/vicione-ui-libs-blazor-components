using Bunit;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Extensions;

public sealed partial class IJSObjectReferenceExtensionsTests
{
    public sealed class InvokeAsync : IAsyncDisposable
    {
        private const string JsModuleIdentifier = "./test-module.js";
        private const string FunctionIdentifier = "testFunction";

        private readonly BunitContext _testContext = new();
        private readonly BunitJSModuleInterop _jsModuleInterop;
        private readonly ILogger _logger = Substitute.For<ILogger>();

        public InvokeAsync()
        {
            _jsModuleInterop = _testContext.JSInterop.SetupModule(JsModuleIdentifier);

            _logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        }

        public ValueTask DisposeAsync()
            => _testContext.DisposeAsync();

        private IEnumerable<LogLevel> GetLoggedLevels()
            => _logger.ReceivedCalls()
                .Where(call => call.GetMethodInfo().Name == nameof(ILogger.Log))
                .Select(call => (LogLevel)call.GetArguments()[0]!);

        private ValueTask<IJSObjectReference> ImportJsModule()
            => _testContext.JSInterop.JSRuntime.InvokeAsync<IJSObjectReference>("import",
                Xunit.TestContext.Current.CancellationToken, JsModuleIdentifier);

        [Fact]
        public async Task Should_return_result()
        {
            // Arrange
            _jsModuleInterop.Setup<string>(FunctionIdentifier).SetResult("result");

            var jsModule = await ImportJsModule();

            // Act
            var result = await jsModule.InvokeAsync<string>(FunctionIdentifier, _logger, Xunit.TestContext.Current.CancellationToken);

            // Assert
            result.Should().Be("result");
        }

        [Fact]
        public async Task Should_pass_identifier_arguments_and_cancellation_token()
        {
            // Arrange
            _jsModuleInterop.Setup<string>(FunctionIdentifier, _ => true).SetResult("result");

            var jsModule = await ImportJsModule();

            using var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await jsModule.InvokeAsync<string>(FunctionIdentifier, _logger, cancellationTokenSource.Token, "arg", 2);

            // Assert
            var invocation = _jsModuleInterop.Invocations[FunctionIdentifier].Should().ContainSingle().Subject;
            invocation.CancellationToken.Should().Be(cancellationTokenSource.Token);
            invocation.Arguments.Should().Equal("arg", 2);
        }

        [Fact]
        public async Task Should_return_default_when_js_object_reference_is_null()
        {
            // Arrange
            IJSObjectReference? jsModule = null;

            // Act
            var result = await jsModule.InvokeAsync<string>(FunctionIdentifier, _logger, Xunit.TestContext.Current.CancellationToken);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Should_return_default_and_log_error_when_function_throws()
        {
            // Arrange
            _jsModuleInterop.Setup<string>(FunctionIdentifier)
                .SetException(new JSException("Cannot read properties of null (reading 'addEventListener')"));

            var jsModule = await ImportJsModule();

            // Act
            var result = await jsModule.InvokeAsync<string>(FunctionIdentifier, _logger, Xunit.TestContext.Current.CancellationToken);

            // Assert
            result.Should().BeNull();
            GetLoggedLevels().Should().Equal(LogLevel.Error);
        }

        [Fact]
        public async Task Should_return_default_without_logging_when_circuit_is_disconnected()
        {
            // Arrange
            _jsModuleInterop.Setup<string>(FunctionIdentifier).SetException(new JSDisconnectedException("Circuit disconnected"));

            var jsModule = await ImportJsModule();

            // Act
            var result = await jsModule.InvokeAsync<string>(FunctionIdentifier, _logger, Xunit.TestContext.Current.CancellationToken);

            // Assert
            result.Should().BeNull();
            GetLoggedLevels().Should().BeEmpty();
        }

        [Fact]
        public async Task Should_return_default_without_logging_when_canceled()
        {
            // Arrange
            _jsModuleInterop.Setup<string>(FunctionIdentifier).SetCanceled();

            var jsModule = await ImportJsModule();

            // Act
            var result = await jsModule.InvokeAsync<string>(FunctionIdentifier, _logger, Xunit.TestContext.Current.CancellationToken);

            // Assert
            result.Should().BeNull();
            GetLoggedLevels().Should().BeEmpty();
        }
    }
}
