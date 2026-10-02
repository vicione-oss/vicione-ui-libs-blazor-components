using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.FocusTrap.Components;
using FocusTrapService = ViciOne.Ui.Blazor.Components.FocusTrap.Services.FocusTrap;

namespace ViciOne.Ui.Blazor.Components.Tests.FocusTrap.Services;

public sealed class FocusTrapTests : IAsyncDisposable
{
    private readonly RecordingJSObjectReference _jsModule = new();
    private readonly TaskCompletionSource<IJSObjectReference> _importCompletionSource = new();
    private readonly ImportingJSRuntime _jsRuntime;
    private readonly FocusTrapService _focusTrap;

    public FocusTrapTests()
    {
        _jsRuntime = new ImportingJSRuntime(_importCompletionSource.Task);
        _focusTrap = new FocusTrapService(Substitute.For<ILogger<FocusTrapService>>(), _jsRuntime);
    }

    public async ValueTask DisposeAsync()
    {
        await _focusTrap.DisposeAsync();
        await _jsModule.DisposeAsync();
    }

    [Fact]
    public async Task Should_not_import_js_module_for_element_not_rendered_yet()
    {
        // Arrange
        _importCompletionSource.SetResult(_jsModule);

        var focusTrappable = new TestFocusTrappable(default);

        // Act
        await _focusTrap.AttachAsync(focusTrappable);

        // Assert
        _jsRuntime.ImportCount.Should().Be(0);
    }

    [Fact]
    public async Task Should_create_single_js_instance_for_repeated_attaches()
    {
        // Arrange
        _importCompletionSource.SetResult(_jsModule);

        var focusTrappable = new TestFocusTrappable(new ElementReference("dialog"));

        // Act
        for (var i = 0; i < 3; i++)
            await _focusTrap.AttachAsync(focusTrappable);

        // Assert
        _jsRuntime.ImportCount.Should().Be(1);

        // Attaching the element that is already trapped must not reach JavaScript at all.
        _jsModule.Instances.Should().ContainSingle()
            .Which.Invocations.Should().ContainSingle().Which.Should().Be("attach");
    }

    [Fact]
    public async Task Should_create_single_js_instance_when_attaching_during_import()
    {
        // Arrange
        var focusTrappable = new TestFocusTrappable(new ElementReference("dialog"));

        // Attaches arriving before the import has completed must not start another one.
        var attachTasks = Enumerable.Range(0, 3)
            .Select(_ => _focusTrap.AttachAsync(focusTrappable))
            .ToList();

        // Act
        _importCompletionSource.SetResult(_jsModule);

        await Task.WhenAll(attachTasks);

        // Assert
        _jsRuntime.ImportCount.Should().Be(1);
        _jsModule.Instances.Should().ContainSingle();
    }

    [Fact]
    public async Task Should_follow_element_rendered_anew()
    {
        // Arrange
        _importCompletionSource.SetResult(_jsModule);

        var focusTrappable = new TestFocusTrappable(new ElementReference("first-dialog"));

        await _focusTrap.AttachAsync(focusTrappable);

        focusTrappable.ElementReference = new ElementReference("second-dialog");

        // Act
        await _focusTrap.AttachAsync(focusTrappable);

        // Assert
        _jsModule.Instances.Should().ContainSingle()
            .Which.AttachedElementReferences.Select(e => e.Id)
            .Should().Equal("first-dialog", "second-dialog");
    }

    [Fact]
    public async Task Should_attach_element_to_new_js_instance_after_remove()
    {
        // Arrange
        _importCompletionSource.SetResult(_jsModule);

        var focusTrappable = new TestFocusTrappable(new ElementReference("first-dialog"));

        await _focusTrap.AttachAsync(focusTrappable);
        await _focusTrap.RemoveAsync(focusTrappable);

        focusTrappable.ElementReference = new ElementReference("second-dialog");

        // Act
        await _focusTrap.AttachAsync(focusTrappable);

        // Assert
        _jsModule.Instances.Should().HaveCount(2);
        _jsModule.Instances[0].Disposed.IsCompleted.Should().BeTrue();
        _jsModule.Instances[1].Disposed.IsCompleted.Should().BeFalse();

        _jsModule.Instances.SelectMany(i => i.AttachedElementReferences).Select(e => e.Id)
            .Should().Equal("first-dialog", "second-dialog");
    }

    [Fact]
    public async Task Should_attach_element_rendered_anew_during_import()
    {
        // Arrange
        var focusTrappable = new TestFocusTrappable(new ElementReference("first-dialog"));

        // The element the first attach waits for the module with is replaced before the import has completed.
        var firstAttachTask = _focusTrap.AttachAsync(focusTrappable);
        var removeTask = _focusTrap.RemoveAsync(focusTrappable);

        focusTrappable.ElementReference = new ElementReference("second-dialog");

        var secondAttachTask = _focusTrap.AttachAsync(focusTrappable);

        // Act
        _importCompletionSource.SetResult(_jsModule);

        await Task.WhenAll(firstAttachTask, removeTask, secondAttachTask);

        // Assert
        // The trap of the stale element is released, and the element that is on the page is trapped instead.
        _jsModule.Instances.Should().HaveCount(2);
        _jsModule.Instances[0].Disposed.IsCompleted.Should().BeTrue();
        _jsModule.Instances[1].Disposed.IsCompleted.Should().BeFalse();
        _jsModule.Instances[1].AttachedElementReferences.Select(e => e.Id).Should().Equal("second-dialog");
    }

    [Fact]
    public async Task Should_dispose_js_instance_on_remove()
    {
        // Arrange
        _importCompletionSource.SetResult(_jsModule);

        var focusTrappable = new TestFocusTrappable(new ElementReference("dialog"));

        await _focusTrap.AttachAsync(focusTrappable);

        // Act
        await _focusTrap.RemoveAsync(focusTrappable);

        // Assert
        var jsInstance = _jsModule.Instances.Should().ContainSingle().Subject;

        jsInstance.Invocations.Should().Contain("dispose");
        jsInstance.Disposed.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Should_dispose_js_instance_when_removed_during_import()
    {
        // Arrange
        var focusTrappable = new TestFocusTrappable(new ElementReference("dialog"));

        var attachTask = _focusTrap.AttachAsync(focusTrappable);
        var removeTask = _focusTrap.RemoveAsync(focusTrappable);

        // Act
        _importCompletionSource.SetResult(_jsModule);

        await Task.WhenAll(attachTask, removeTask);

        // Assert
        // The instance is created for an element that has already been removed, so it must not stay attached.
        _jsModule.Instances.Should().ContainSingle()
            .Which.Disposed.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Should_dispose_js_instances_and_module_on_dispose()
    {
        // Arrange
        _importCompletionSource.SetResult(_jsModule);

        var focusTrappable = new TestFocusTrappable(new ElementReference("dialog"));

        await _focusTrap.AttachAsync(focusTrappable);

        // Act
        await _focusTrap.DisposeAsync();

        // Assert
        var jsInstance = _jsModule.Instances.Should().ContainSingle().Subject;

        jsInstance.Invocations.Should().Contain("dispose");
        jsInstance.Disposed.IsCompleted.Should().BeTrue();
        _jsModule.Disposed.IsCompleted.Should().BeTrue();
    }

    private sealed class TestFocusTrappable(ElementReference elementReference) : IFocusTrappable
    {
        public ElementReference ElementReference { get; set; } = elementReference;

        public ElementReference GetElementReference() => ElementReference;
    }

    /// <summary>
    /// Stands in for the JavaScript runtime, handing out the module only once the test releases it, so that
    /// calls arriving while the module is still loading can be tested.
    /// </summary>
    private sealed class ImportingJSRuntime(Task<IJSObjectReference> moduleTask) : IJSRuntime
    {
        public int ImportCount { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken,
            object?[]? args)
        {
            if (identifier != "import")
                return default!;

            ImportCount++;

            return (TValue)await moduleTask;
        }
    }

    /// <summary>
    /// Stands in for an imported JavaScript module and the objects its constructors create, recording what
    /// the focus trap invokes on them.
    /// </summary>
    private sealed class RecordingJSObjectReference : IJSObjectReference
    {
        private readonly TaskCompletionSource _disposedCompletionSource = new();

        public List<string> Invocations { get; } = [];

        public List<RecordingJSObjectReference> Instances { get; } = [];

        public List<ElementReference> AttachedElementReferences { get; } = [];

        public Task Disposed => _disposedCompletionSource.Task;

        public ValueTask<IJSObjectReference> InvokeConstructorAsync(string identifier,
            CancellationToken cancellationToken, object?[]? args)
        {
            Invocations.Add(identifier);

            var instance = new RecordingJSObjectReference();

            Instances.Add(instance);

            return ValueTask.FromResult<IJSObjectReference>(instance);
        }

        public ValueTask<IJSObjectReference> InvokeConstructorAsync(string identifier, object?[]? args)
            => InvokeConstructorAsync(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken,
            object?[]? args)
        {
            Invocations.Add(identifier);

            if (identifier == "attach")
                AttachedElementReferences.AddRange(args?.OfType<ElementReference>() ?? []);

            return ValueTask.FromResult<TValue>(default!);
        }

        public ValueTask DisposeAsync()
        {
            _disposedCompletionSource.TrySetResult();

            return ValueTask.CompletedTask;
        }
    }
}
