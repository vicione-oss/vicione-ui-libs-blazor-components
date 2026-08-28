using System.Linq.Expressions;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;
using ViciOne.Ui.Blazor.Components.Resizeable.Components;
using ViciOne.Ui.Blazor.Components.Resizeable.Enums;
using ViciOne.Ui.Blazor.Components.Resizeable.Extensions;
using ViciOne.Ui.Blazor.Components.Resizeable.Models;
using ViciOne.Ui.Blazor.Components.Resizeable.Services;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Resizeable.Extensions;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Resizeable.Services;

public sealed class ResizeableInteractionTests
{
    private readonly string _jsModuleIdentifier =
        $"./_content/{typeof(ResizeInteraction).Assembly.GetName().Name}/resizeable/resize-interaction.js";

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task Assert_js_module_import_on_attach_async(bool isAlreadyAttached, bool shouldImportJsModule)
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();
        var resizeable = new TestResizeable();
        var skipInvocationCount = 0;

        if (isAlreadyAttached)
        {
            await resizeInteraction.AttachAsync(resizeable);

            skipInvocationCount = testContext.JSInterop.Invocations.Count;
        }

        // Act
        await resizeInteraction.AttachAsync(resizeable);

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
    public async Task Should_invoke_js_attach_method_on_attach_async(bool isAlreadyAttached, bool withPointerCaptureBehaviors,
        bool shouldInvokeJsAttach)
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);

        jsModule.SetupModule("attach", _ => true);

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();
        var resizeable = new TestResizeable();

        var pointerCaptureBehaviorJsObject = Substitute.For<IJSObjectReference>();
        var pointerCaptureBehavior = Substitute.For<IPointerCaptureBehavior>();
        pointerCaptureBehavior.GetJsObjectAsync()
            .Returns(Task.FromResult<IJSObjectReference?>(pointerCaptureBehaviorJsObject));

        var skipInvocationCount = 0;

        IEnumerable<IPointerCaptureBehavior>? pointerCaptureBehaviors =
            withPointerCaptureBehaviors ? [pointerCaptureBehavior] : null;

        if (isAlreadyAttached)
        {
            await resizeInteraction.AttachAsync(resizeable, pointerCaptureBehaviors);

            skipInvocationCount = jsModule.Invocations.Count;
        }

        // Act
        await resizeInteraction.AttachAsync(resizeable, pointerCaptureBehaviors);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "attach";

        var invocations = jsModule.Invocations.ToList();
        if (isAlreadyAttached)
            invocations = [.. invocations.Skip(skipInvocationCount)];

        if (shouldInvokeJsAttach)
        {
            var invocation = jsModule.Invocations.First(invocationMatcher.Compile());
            var argument = invocation.Arguments.OfType<ResizeInteractionContext>().First();
            argument.Resizeable.Should().Be(resizeable.GetElementReference());
            var resizeHandle = argument.ResizeHandles.Should().ContainSingle().Subject;
            resizeHandle.Element.Should().Be(resizeable.GetResizeHandles().Single().GetElementReference());
            resizeHandle.Position.Should().Be(resizeable.Position);
            argument.ResizeContainer.Should().Be(resizeable.GetResizeContainer().GetElementReference());
            argument.StartedCssClass.Should().Be(resizeInteraction.StartedCssClass);
            argument.OngoingCssClass.Should().Be(resizeInteraction.OngoingCssClass);
            argument.EndedCssClass.Should().Be(resizeInteraction.EndedCssClass);

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
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();

        await ((IAsyncDisposable)resizeInteraction).DisposeAsync();

        // Act & Assert - should not throw
        var resizeable = new TestResizeable();
        var action = async () => await resizeInteraction.AttachAsync(resizeable);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_not_throw_when_remove_called_after_dispose()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();

        await ((IAsyncDisposable)resizeInteraction).DisposeAsync();

        // Act & Assert - should not throw
        var resizeable = new TestResizeable();
        var action = async () => await resizeInteraction.RemoveAsync(resizeable);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_only_dispose_once()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();
        var disposable = (IAsyncDisposable)resizeInteraction;

        // Act
        await disposable.DisposeAsync();

        // Assert - second dispose should not throw
        var action = async () => await disposable.DisposeAsync();
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_invoke_dispose_on_js_attach_result_on_dispose()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();
        var resizeable = new TestResizeable();

        await resizeInteraction.AttachAsync(resizeable);

        // Act
        await ((IAsyncDisposable)resizeInteraction).DisposeAsync();

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "dispose";
        testContext.JSInterop.Invocations.Should().Contain(invocationMatcher);
    }

    [Fact]
    public async Task Should_invoke_dispose_on_js_attach_result_when_remove_async()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();
        var resizeable = new TestResizeable();

        await resizeInteraction.AttachAsync(resizeable);

        // Act
        await resizeInteraction.RemoveAsync(resizeable);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "dispose";
        testContext.JSInterop.Invocations.Should().Contain(invocationMatcher);
    }

    [Fact]
    public async Task Should_update_resizeable_position_and_size_on_pointer_up()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();
        var resizeable = new TestResizeable();

        await resizeInteraction.AttachAsync(resizeable);

        var invocation = jsModule.Invocations.First(i => i.Identifier == "attach");
        var context = invocation.Arguments.OfType<ResizeInteractionContext>().First();

        // Act
        await ((ResizeInteraction)resizeInteraction).OnUpdatePositionAndSizeAsync(context.ResizeableId,
            new() { X = 10, Y = 20, Width = 100, Height = 200 });

        // Assert
        resizeable.DomRect.X.Should().Be(10);
        resizeable.DomRect.Y.Should().Be(20);
        resizeable.DomRect.Width.Should().Be(100);
        resizeable.DomRect.Height.Should().Be(200);
    }

    [Fact]
    public async Task Should_not_throw_when_removing_non_attached_resizeable()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();
        var resizeable = new TestResizeable();

        // Act & Assert - should not throw
        var action = async () => await resizeInteraction.RemoveAsync(resizeable);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_invoke_add_pointer_capture_behavior_on_js_attach_result()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();
        var resizeable = new TestResizeable();

        var pointerCaptureBehaviorJsObject = Substitute.For<IJSObjectReference>();
        var pointerCaptureBehavior = Substitute.For<IPointerCaptureBehavior>();
        pointerCaptureBehavior.GetJsObjectAsync()
            .Returns(Task.FromResult<IJSObjectReference?>(pointerCaptureBehaviorJsObject));

        await resizeInteraction.AttachAsync(resizeable);

        // Act
        await resizeInteraction.AddPointerCaptureBehaviorAsync(resizeable, pointerCaptureBehavior);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "addPointerCaptureBehavior";
        testContext.JSInterop.Invocations.Should().Contain(invocationMatcher);
    }

    [Fact]
    public async Task Should_invoke_remove_pointer_capture_behavior_on_js_attach_result()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();
        var resizeable = new TestResizeable();

        var pointerCaptureBehaviorJsObject = Substitute.For<IJSObjectReference>();
        var pointerCaptureBehavior = Substitute.For<IPointerCaptureBehavior>();
        pointerCaptureBehavior.GetJsObjectAsync()
            .Returns(Task.FromResult<IJSObjectReference?>(pointerCaptureBehaviorJsObject));

        await resizeInteraction.AttachAsync(resizeable);

        // Act
        await resizeInteraction.RemovePointerCaptureBehaviorAsync(resizeable, pointerCaptureBehavior);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "removePointerCaptureBehavior";
        testContext.JSInterop.Invocations.Should().Contain(invocationMatcher);
    }

    [Fact]
    public async Task Should_not_throw_when_add_pointer_capture_behavior_called_after_dispose()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();

        var pointerCaptureBehavior = Substitute.For<IPointerCaptureBehavior>();

        await ((IAsyncDisposable)resizeInteraction).DisposeAsync();

        // Act & Assert - should not throw
        var resizeable = new TestResizeable();
        var action = async () => await resizeInteraction.AddPointerCaptureBehaviorAsync(resizeable, pointerCaptureBehavior);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_not_throw_when_remove_pointer_capture_behavior_called_after_dispose()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();

        var pointerCaptureBehavior = Substitute.For<IPointerCaptureBehavior>();

        await ((IAsyncDisposable)resizeInteraction).DisposeAsync();

        // Act & Assert - should not throw
        var resizeable = new TestResizeable();
        var action = async () => await resizeInteraction.RemovePointerCaptureBehaviorAsync(resizeable, pointerCaptureBehavior);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_not_invoke_add_when_pointer_capture_behavior_returns_null()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();
        var resizeable = new TestResizeable();

        var pointerCaptureBehavior = Substitute.For<IPointerCaptureBehavior>();
        pointerCaptureBehavior.GetJsObjectAsync()
            .Returns(Task.FromResult<IJSObjectReference?>(null));

        await resizeInteraction.AttachAsync(resizeable);

        // Act
        await resizeInteraction.AddPointerCaptureBehaviorAsync(resizeable, pointerCaptureBehavior);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "addPointerCaptureBehavior";
        testContext.JSInterop.Invocations.Should().NotContain(invocationMatcher);
    }

    [Fact]
    public async Task Should_not_invoke_add_when_resizeable_not_attached()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForResizeInteraction()
            .AddResizeable();

        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var resizeInteraction = testContext.Services.GetRequiredService<IResizeInteraction>();
        var resizeable = new TestResizeable();

        var pointerCaptureBehaviorJsObject = Substitute.For<IJSObjectReference>();
        var pointerCaptureBehavior = Substitute.For<IPointerCaptureBehavior>();
        pointerCaptureBehavior.GetJsObjectAsync()
            .Returns(Task.FromResult<IJSObjectReference?>(pointerCaptureBehaviorJsObject));

        // Act - resizeable is not attached
        await resizeInteraction.AddPointerCaptureBehaviorAsync(resizeable, pointerCaptureBehavior);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> invocationMatcher = i => i.Identifier == "addPointerCaptureBehavior";
        testContext.JSInterop.Invocations.Should().NotContain(invocationMatcher);
    }

    private sealed class TestResizeContainer : IResizeContainer
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private readonly ElementReference _elementReference;
#pragma warning restore CS0649

        public ElementReference GetElementReference() => _elementReference;
    }

    private sealed class TestResizeable : IResizeable, IResizeHandle
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private readonly ElementReference _elementReference;
#pragma warning restore CS0649
        private readonly TestResizeContainer _resizeContainer = new();

        public bool Resizeable { get; set; }
        public DomRect DomRect { get; set; } = new();
        public ResizeHandlePosition Position => ResizeHandlePosition.TopLeft;

        public ElementReference GetElementReference() => _elementReference;
        public IResizeContainer GetResizeContainer() => _resizeContainer;
        public IReadOnlyCollection<IResizeHandle> GetResizeHandles() => [this];
        public double GetMinimumWidth() => 40;
        public double GetMinimumHeight() => 40;

        public void RegisterResizeHandle(IResizeHandle resizeHandle)
        {
        }

        public void UnregisterResizeHandle(IResizeHandle resizeHandle)
        {
        }

        public Task UpdatePositionAndSizeAsync(DomRect domRect)
        {
            DomRect = domRect;
            return Task.CompletedTask;
        }
    }
}
