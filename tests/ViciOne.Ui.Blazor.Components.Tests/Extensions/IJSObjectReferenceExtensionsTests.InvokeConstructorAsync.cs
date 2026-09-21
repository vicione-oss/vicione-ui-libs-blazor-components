using Bunit;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Extensions;

public sealed class IJSObjectReferenceExtensionsTests
{
    public sealed class InvokeConstructorAsync : IAsyncDisposable
    {
        private const string JsModuleIdentifier = "./test-module.js";
        private const string ClassIdentifier = "TestClass";

        private readonly BunitContext _testContext = new();
        private readonly ILogger _logger = Substitute.For<ILogger>();

        public InvokeConstructorAsync()
            => _logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

        public ValueTask DisposeAsync()
            => _testContext.DisposeAsync();

        private IEnumerable<LogLevel> GetLoggedLevels()
            => _logger.ReceivedCalls()
                .Where(call => call.GetMethodInfo().Name == nameof(ILogger.Log))
                .Select(call => (LogLevel)call.GetArguments()[0]!);

        [Fact]
        public async Task Should_return_created_object()
        {
            // Arrange
            var jsModuleInterop = _testContext.JSInterop.SetupModule(JsModuleIdentifier);
            jsModuleInterop.SetupModule(ClassIdentifier, _ => true);

            var jsModule = await _testContext.JSInterop.JSRuntime.InvokeAsync<IJSObjectReference>("import", JsModuleIdentifier);

            // Act
            var result = await jsModule.InvokeConstructorAsync(ClassIdentifier, _logger);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_pass_identifier_arguments_and_cancellation_token()
        {
            // Arrange
            await using var jsModule = new ConstructorThrowingJSObjectReference(new JSDisconnectedException("Circuit disconnected"));

            using var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await jsModule.InvokeConstructorAsync(ClassIdentifier, _logger, cancellationTokenSource.Token, "arg", 2);

            // Assert
            var invocation = jsModule.ConstructorInvocations.Should().ContainSingle().Subject;
            invocation.Identifier.Should().Be(ClassIdentifier);
            invocation.CancellationToken.Should().Be(cancellationTokenSource.Token);
            invocation.Args.Should().Equal("arg", 2);
        }

        [Fact]
        public async Task Should_not_pass_logger_as_argument_when_no_cancellation_token_is_given()
        {
            // Arrange
            await using var jsModule = new ConstructorThrowingJSObjectReference(new JSDisconnectedException("Circuit disconnected"));

            // Act
            await jsModule.InvokeConstructorAsync(ClassIdentifier, _logger, "arg");

            // Assert
            jsModule.ConstructorInvocations.Should().ContainSingle()
                .Which.Args.Should().Equal("arg");
        }

        [Fact]
        public async Task Should_return_null_when_js_object_reference_is_null()
        {
            // Arrange
            IJSObjectReference? jsModule = null;

            // Act
            var result = await jsModule.InvokeConstructorAsync(ClassIdentifier, _logger);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Should_return_null_and_log_error_when_constructor_throws()
        {
            // Arrange
            await using var jsModule = new ConstructorThrowingJSObjectReference(
                new JSException("Cannot read properties of null (reading 'parentElement')"));

            // Act
            var result = await jsModule.InvokeConstructorAsync(ClassIdentifier, _logger);

            // Assert
            result.Should().BeNull();
            GetLoggedLevels().Should().Equal(LogLevel.Error);
        }

        [Fact]
        public async Task Should_return_null_without_logging_when_circuit_is_disconnected()
        {
            // Arrange
            await using var jsModule = new ConstructorThrowingJSObjectReference(new JSDisconnectedException("Circuit disconnected"));

            // Act
            var result = await jsModule.InvokeConstructorAsync(ClassIdentifier, _logger);

            // Assert
            result.Should().BeNull();
            GetLoggedLevels().Should().BeEmpty();
        }

        [Fact]
        public async Task Should_return_null_without_logging_when_canceled()
        {
            // Arrange
            await using var jsModule = new ConstructorThrowingJSObjectReference(new OperationCanceledException());

            // Act
            var result = await jsModule.InvokeConstructorAsync(ClassIdentifier, _logger);

            // Assert
            result.Should().BeNull();
            GetLoggedLevels().Should().BeEmpty();
        }
    }
}
