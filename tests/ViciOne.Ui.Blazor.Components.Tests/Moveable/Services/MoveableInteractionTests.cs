using System.Linq.Expressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Moveable.Components;
using ViciOne.Ui.Blazor.Components.Moveable.Extensions;
using ViciOne.Ui.Blazor.Components.Moveable.Models;
using ViciOne.Ui.Blazor.Components.Moveable.Services;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Moveable.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Moveable.Services;

public sealed class MoveableInteractionTests
{
    private readonly string _jsModuleIdentifier =
        $"./_content/{typeof(MoveInteraction).Assembly.GetName().Name}/moveable/move-interaction.js";

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task Assert_js_module_import_on_attach_async(bool isAlreadyAttached, bool shouldImportJsModule)
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForMoveInteraction()
            .AddMoveable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var moveInteraction = testContext.Services.GetRequiredService<IMoveInteraction>();
        var moveable = new TestMoveable();
        var skipInvocationCount = 0;

        if (isAlreadyAttached)
        {
            await moveInteraction.AttachAsync(moveable);

            skipInvocationCount = testContext.JSInterop.Invocations.Count;
        }

        // Act
        await moveInteraction.AttachAsync(moveable);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "import" &&
            i.Arguments.OfType<string>().First() == _jsModuleIdentifier;

        var invocations = testContext.JSInterop.Invocations.ToList();
        if (isAlreadyAttached)
            invocations = [.. invocations.Skip(skipInvocationCount)];

        var should = invocations.Should();

        if (shouldImportJsModule)
            should.Contain(invocationMatcher);
        else
            should.NotContain(invocationMatcher);
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    public async Task Should_invoke_js_constructor_on_attach_async(bool isAlreadyAttached, bool withPointerCaptureBehaviors,
        bool shouldInvokeJsConstructor)
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForMoveInteraction()
            .AddMoveable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);

        var jsInstance = jsModule.SetupModule("MoveInteraction", _ => true);

        var moveInteraction = testContext.Services.GetRequiredService<IMoveInteraction>();
        var moveable = new TestMoveable();

        var pointerCaptureBehaviorJsObject = Substitute.For<IJSObjectReference>();
        var pointerCaptureBehavior = Substitute.For<IPointerCaptureBehavior>();
        pointerCaptureBehavior.GetJsObjectAsync()
            .Returns(Task.FromResult<IJSObjectReference?>(pointerCaptureBehaviorJsObject));

        var skipInvocationCount = 0;

        IEnumerable<IPointerCaptureBehavior>? pointerCaptureBehaviors =
            withPointerCaptureBehaviors ? [pointerCaptureBehavior] : null;

        if (isAlreadyAttached)
        {
            await moveInteraction.AttachAsync(moveable, pointerCaptureBehaviors);

            skipInvocationCount = jsModule.Invocations.Count;
        }

        // Act
        await moveInteraction.AttachAsync(moveable, pointerCaptureBehaviors);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "MoveInteraction";

        var invocations = jsModule.Invocations.ToList();
        if (isAlreadyAttached)
            invocations = [.. invocations.Skip(skipInvocationCount)];

        if (shouldInvokeJsConstructor)
        {
            var invocation = jsModule.Invocations.First(invocationMatcher.Compile());
            var argument = invocation.Arguments.OfType<MoveInteractionContext>().First();
            argument.Moveable.Should().Be(moveable.GetElementReference());
            argument.MoveHandle.Should().Be(moveable.GetMoveHandle().GetElementReference());
            argument.MoveContainer.Should().Be(moveable.GetMoveContainer().GetElementReference());
            argument.StartedCssClass.Should().Be(moveInteraction.StartedCssClass);
            argument.OngoingCssClass.Should().Be(moveInteraction.OngoingCssClass);
            argument.EndedCssClass.Should().Be(moveInteraction.EndedCssClass);

            if (withPointerCaptureBehaviors)
                argument.PointerCaptureBehaviors.Should().BeEquivalentTo([pointerCaptureBehaviorJsObject]);
            else
                argument.PointerCaptureBehaviors.Should().BeNull();
        }
        else
        {
            invocations.Should().NotContain(invocationMatcher);
        }
    }

    [Fact]
    public async Task Should_not_throw_when_attach_called_after_dispose()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForMoveInteraction()
            .AddMoveable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var moveInteraction = testContext.Services.GetRequiredService<IMoveInteraction>();

        await ((IAsyncDisposable)moveInteraction).DisposeAsync();

        // Act & Assert - should not throw
        var moveable = new TestMoveable();
        var action = async () => await moveInteraction.AttachAsync(moveable);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_not_throw_when_remove_called_after_dispose()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForMoveInteraction()
            .AddMoveable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var moveInteraction = testContext.Services.GetRequiredService<IMoveInteraction>();

        await ((IAsyncDisposable)moveInteraction).DisposeAsync();

        // Act & Assert - should not throw
        var moveable = new TestMoveable();
        var action = async () => await moveInteraction.RemoveAsync(moveable);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_only_dispose_once()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForMoveInteraction()
            .AddMoveable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var moveInteraction = testContext.Services.GetRequiredService<IMoveInteraction>();
        var disposable = (IAsyncDisposable)moveInteraction;

        // Act
        await disposable.DisposeAsync();

        // Assert - second dispose should not throw
        var action = async () => await disposable.DisposeAsync();
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_invoke_dispose_on_js_instance_on_dispose()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForMoveInteraction()
            .AddMoveable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("MoveInteraction", _ => true);

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var moveInteraction = testContext.Services.GetRequiredService<IMoveInteraction>();
        var moveable = new TestMoveable();

        await moveInteraction.AttachAsync(moveable);

        // Act
        await ((IAsyncDisposable)moveInteraction).DisposeAsync();

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "dispose";
        testContext.JSInterop.Invocations.Should().Contain(invocationMatcher);
    }

    [Fact]
    public async Task Should_invoke_dispose_on_js_instance_when_remove_async()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForMoveInteraction()
            .AddMoveable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("MoveInteraction", _ => true);

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var moveInteraction = testContext.Services.GetRequiredService<IMoveInteraction>();
        var moveable = new TestMoveable();

        await moveInteraction.AttachAsync(moveable);

        // Act
        await moveInteraction.RemoveAsync(moveable);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "dispose";
        testContext.JSInterop.Invocations.Should().Contain(invocationMatcher);
    }

    [Fact]
    public async Task Should_not_throw_when_removing_non_attached_moveable()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForMoveInteraction()
            .AddMoveable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var moveInteraction = testContext.Services.GetRequiredService<IMoveInteraction>();
        var moveable = new TestMoveable();

        // Act & Assert - should not throw
        var action = async () => await moveInteraction.RemoveAsync(moveable);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_invoke_add_pointer_capture_behavior_on_js_instance()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForMoveInteraction()
            .AddMoveable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("MoveInteraction", _ => true);

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var moveInteraction = testContext.Services.GetRequiredService<IMoveInteraction>();
        var moveable = new TestMoveable();

        var pointerCaptureBehaviorJsObject = Substitute.For<IJSObjectReference>();
        var pointerCaptureBehavior = Substitute.For<IPointerCaptureBehavior>();
        pointerCaptureBehavior.GetJsObjectAsync()
            .Returns(Task.FromResult<IJSObjectReference?>(pointerCaptureBehaviorJsObject));

        await moveInteraction.AttachAsync(moveable);

        // Act
        await moveInteraction.AddPointerCaptureBehaviorAsync(moveable, pointerCaptureBehavior);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "addPointerCaptureBehavior";
        testContext.JSInterop.Invocations.Should().Contain(invocationMatcher);
    }

    [Fact]
    public async Task Should_invoke_remove_pointer_capture_behavior_on_js_instance()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForMoveInteraction()
            .AddMoveable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("MoveInteraction", _ => true);

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var moveInteraction = testContext.Services.GetRequiredService<IMoveInteraction>();
        var moveable = new TestMoveable();

        var pointerCaptureBehaviorJsObject = Substitute.For<IJSObjectReference>();
        var pointerCaptureBehavior = Substitute.For<IPointerCaptureBehavior>();
        pointerCaptureBehavior.GetJsObjectAsync()
            .Returns(Task.FromResult<IJSObjectReference?>(pointerCaptureBehaviorJsObject));

        await moveInteraction.AttachAsync(moveable);

        // Act
        await moveInteraction.RemovePointerCaptureBehaviorAsync(moveable, pointerCaptureBehavior);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "removePointerCaptureBehavior";
        testContext.JSInterop.Invocations.Should().Contain(invocationMatcher);
    }

    [Fact]
    public async Task Should_not_throw_when_add_pointer_capture_behavior_called_after_dispose()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForMoveInteraction()
            .AddMoveable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var moveInteraction = testContext.Services.GetRequiredService<IMoveInteraction>();

        var pointerCaptureBehavior = Substitute.For<IPointerCaptureBehavior>();

        await ((IAsyncDisposable)moveInteraction).DisposeAsync();

        // Act & Assert - should not throw
        var moveable = new TestMoveable();
        var action = async () => await moveInteraction.AddPointerCaptureBehaviorAsync(moveable, pointerCaptureBehavior);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_not_throw_when_remove_pointer_capture_behavior_called_after_dispose()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForMoveInteraction()
            .AddMoveable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var moveInteraction = testContext.Services.GetRequiredService<IMoveInteraction>();

        var pointerCaptureBehavior = Substitute.For<IPointerCaptureBehavior>();

        await ((IAsyncDisposable)moveInteraction).DisposeAsync();

        // Act & Assert - should not throw
        var moveable = new TestMoveable();
        var action = async () => await moveInteraction.RemovePointerCaptureBehaviorAsync(moveable, pointerCaptureBehavior);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_not_invoke_add_when_pointer_capture_behavior_returns_null()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForMoveInteraction()
            .AddMoveable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("MoveInteraction", _ => true);

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var moveInteraction = testContext.Services.GetRequiredService<IMoveInteraction>();
        var moveable = new TestMoveable();

        var pointerCaptureBehavior = Substitute.For<IPointerCaptureBehavior>();
        pointerCaptureBehavior.GetJsObjectAsync()
            .Returns(Task.FromResult<IJSObjectReference?>(null));

        await moveInteraction.AttachAsync(moveable);

        // Act
        await moveInteraction.AddPointerCaptureBehaviorAsync(moveable, pointerCaptureBehavior);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "addPointerCaptureBehavior";
        testContext.JSInterop.Invocations.Should().NotContain(invocationMatcher);
    }

    [Fact]
    public async Task Should_not_invoke_add_when_moveable_not_attached()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForMoveInteraction()
            .AddMoveable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var moveInteraction = testContext.Services.GetRequiredService<IMoveInteraction>();
        var moveable = new TestMoveable();

        var pointerCaptureBehaviorJsObject = Substitute.For<IJSObjectReference>();
        var pointerCaptureBehavior = Substitute.For<IPointerCaptureBehavior>();
        pointerCaptureBehavior.GetJsObjectAsync()
            .Returns(Task.FromResult<IJSObjectReference?>(pointerCaptureBehaviorJsObject));

        // Act - moveable is not attached
        await moveInteraction.AddPointerCaptureBehaviorAsync(moveable, pointerCaptureBehavior);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "addPointerCaptureBehavior";
        testContext.JSInterop.Invocations.Should().NotContain(invocationMatcher);
    }

    private sealed class TestMoveContainer : IMoveContainer
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private readonly ElementReference _elementReference;
#pragma warning restore CS0649

        public ElementReference GetElementReference() => _elementReference;
    }

    private sealed class TestMoveable : IMoveable, IMoveHandle
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private readonly ElementReference _elementReference;
#pragma warning restore CS0649
        private readonly TestMoveContainer _moveContainer = new();

        public bool Moveable { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public ElementReference GetElementReference() => _elementReference;
        public IMoveContainer GetMoveContainer() => _moveContainer;
        public IMoveHandle GetMoveHandle() => this;

        public async Task UpdatePositionAsync(double x, double y)
        {
            X = x;
            Y = y;

            await Task.CompletedTask;
        }
    }
}
