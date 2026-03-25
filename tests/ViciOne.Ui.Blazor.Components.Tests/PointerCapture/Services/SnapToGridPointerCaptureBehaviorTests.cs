using System.Linq.Expressions;
using AwesomeAssertions;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PointerCapture.Extensions;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;
using ViciOne.Ui.Blazor.Components.TestingHelpers.PointerCapture.Extensions;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PointerCapture.Services;

public sealed class SnapToGridPointerCaptureBehaviorTests
{
    private readonly string _jsModuleIdentifier =
        $"./_content/{typeof(SnapToGridPointerCaptureBehavior).Assembly.GetName().Name}/pointer-capture/snap-to-grid-pointer-capture-behavior.js";

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task Assert_js_module_import_on_get_js_object_async(bool isAlreadyInitialized, bool shouldImportJsModule)
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForSnapToGridPointerCaptureBehavior()
            .AddSnapToGridPointerCaptureBehavior();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var behavior = testContext.Services.GetRequiredService<ISnapToGridPointerCaptureBehavior>();
        var skipInvocationCount = 0;

        if (isAlreadyInitialized)
        {
            await behavior.GetJsObjectAsync();

            skipInvocationCount = testContext.JSInterop.Invocations.Count;
        }

        // Act
        await behavior.GetJsObjectAsync();

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "import" &&
            i.Arguments.OfType<string>().First() == _jsModuleIdentifier;

        var invocations = testContext.JSInterop.Invocations.ToList();
        if (isAlreadyInitialized)
            invocations = [.. invocations.Skip(skipInvocationCount)];

        var should = invocations.Should();

        if (shouldImportJsModule)
            should.Contain(invocationMatcher);
        else
            should.NotContain(invocationMatcher);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task Should_invoke_create_instance_on_get_js_object_async(bool isAlreadyInitialized, bool shouldInvokeCreateInstance)
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForSnapToGridPointerCaptureBehavior()
            .AddSnapToGridPointerCaptureBehavior();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("createInstance", _ => true);

        var behavior = testContext.Services.GetRequiredService<ISnapToGridPointerCaptureBehavior>();
        var skipInvocationCount = 0;

        if (isAlreadyInitialized)
        {
            await behavior.GetJsObjectAsync();

            skipInvocationCount = jsModule.Invocations.Count;
        }

        // Act
        await behavior.GetJsObjectAsync();

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "createInstance";

        var invocations = jsModule.Invocations.ToList();
        if (isAlreadyInitialized)
            invocations = [.. invocations.Skip(skipInvocationCount)];

        var should = invocations.Should();

        if (shouldInvokeCreateInstance)
            should.Contain(invocationMatcher);
        else
            should.NotContain(invocationMatcher);
    }

    [Fact]
    public async Task Should_pass_grid_size_to_create_instance()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForSnapToGridPointerCaptureBehavior()
            .AddSnapToGridPointerCaptureBehavior();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("createInstance", _ => true);

        var behavior = testContext.Services.GetRequiredService<ISnapToGridPointerCaptureBehavior>();

        const int GridSize = 15;

        await behavior.SetGridSizeAsync(GridSize);

        // Act
        await behavior.GetJsObjectAsync();

        // Assert
        var invocation = jsModule.Invocations.First(i => i.Identifier == "createInstance");
        invocation.Arguments.Should().Contain(GridSize);
    }

    [Fact]
    public async Task Should_return_null_when_disposed()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForSnapToGridPointerCaptureBehavior()
            .AddSnapToGridPointerCaptureBehavior();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var behavior = testContext.Services.GetRequiredService<ISnapToGridPointerCaptureBehavior>();

        await ((IAsyncDisposable)behavior).DisposeAsync();

        // Act
        var result = await behavior.GetJsObjectAsync();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task Should_set_grid_size(int gridSize)
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForSnapToGridPointerCaptureBehavior()
            .AddSnapToGridPointerCaptureBehavior();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var behavior = testContext.Services.GetRequiredService<ISnapToGridPointerCaptureBehavior>();

        // Act
        await behavior.SetGridSizeAsync(gridSize);

        // Assert - the grid size should be stored and used when creating the JS object
        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("createInstance", _ => true);

        await behavior.GetJsObjectAsync();

        var invocation = jsModule.Invocations.FirstOrDefault(i => i.Identifier == "createInstance");
        invocation.Arguments.Should().Contain(gridSize);
    }

    [Theory]
    [InlineData(10)]
    public async Task Should_not_update_js_object_when_grid_size_unchanged(int gridSize)
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForSnapToGridPointerCaptureBehavior()
            .AddSnapToGridPointerCaptureBehavior();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("createInstance", _ => true);

        var behavior = testContext.Services.GetRequiredService<ISnapToGridPointerCaptureBehavior>();

        await behavior.SetGridSizeAsync(gridSize);
        await behavior.GetJsObjectAsync();

        var invocationCountAfterFirstSet = jsModule.Invocations.Count;

        // Act
        await behavior.SetGridSizeAsync(gridSize); // Same value

        // Assert
        jsModule.Invocations.Count.Should().Be(invocationCountAfterFirstSet);
    }

    [Fact]
    public async Task Should_invoke_set_grid_size_on_js_object_when_already_initialized()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForSnapToGridPointerCaptureBehavior()
            .AddSnapToGridPointerCaptureBehavior();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("createInstance", _ => true);

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var behavior = testContext.Services.GetRequiredService<ISnapToGridPointerCaptureBehavior>();

        await behavior.GetJsObjectAsync();

        // Act
        await behavior.SetGridSizeAsync(20);

        // Assert - setGridSize should be called on the JS object
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "setGridSize";
        testContext.JSInterop.Invocations.Should().Contain(invocationMatcher);
    }

    [Fact]
    public async Task Should_not_throw_when_set_grid_size_called_after_dispose()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForSnapToGridPointerCaptureBehavior()
            .AddSnapToGridPointerCaptureBehavior();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var behavior = testContext.Services.GetRequiredService<ISnapToGridPointerCaptureBehavior>();

        await ((IAsyncDisposable)behavior).DisposeAsync();

        // Act & Assert - should not throw
        var action = async () => await behavior.SetGridSizeAsync(10);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_only_dispose_once()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForSnapToGridPointerCaptureBehavior()
            .AddSnapToGridPointerCaptureBehavior();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var behavior = testContext.Services.GetRequiredService<ISnapToGridPointerCaptureBehavior>();
        var disposable = (IAsyncDisposable)behavior;

        // Act
        await disposable.DisposeAsync();

        // Assert - second dispose should not throw
        var action = async () => await disposable.DisposeAsync();
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_dispose_js_references_on_dispose()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForSnapToGridPointerCaptureBehavior()
            .AddSnapToGridPointerCaptureBehavior();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("createInstance", _ => true);

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var behavior = testContext.Services.GetRequiredService<ISnapToGridPointerCaptureBehavior>();

        await behavior.GetJsObjectAsync();

        // Act & Assert - disposal should complete without throwing
        var action = async () => await ((IAsyncDisposable)behavior).DisposeAsync();
        await action.Should().NotThrowAsync();
    }
}
