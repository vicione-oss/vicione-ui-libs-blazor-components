using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shared.Pages.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Draggable.Components;
using Xunit;

namespace Shared.Tests.Pages.Draggable.Components;

public class DropzoneTests
{
    private readonly DropzoneTests<Dropzone, IDropzone> _dropzoneTests =
        new(".dropzone");

    private static void ConfigureServices(IServiceCollection services)
        => services.AddScoped(_ => Substitute.For<ISnapToGridPointerCaptureBehavior>());

    private static void ConfigureComponentParameters(ComponentParameterCollectionBuilder<Dropzone> p)
        => p.Add(c => c.Label, "test");

    [Fact]
    public void Assert_event_handler_for_drag_start_assigned()
        => _dropzoneTests.ShouldAssignEventHandlerForDragStart(ConfigureServices, ConfigureComponentParameters);

    [Fact]
    public async Task Assert_event_handler_for_drag_start_removed_on_dispose()
        => await _dropzoneTests.ShouldRemoveEventHandlerForDragStartOnDisposeAsync(ConfigureServices, ConfigureComponentParameters);

    [Fact]
    public void Assert_indicate_dropzone_and_register_on_drag_start()
    {
        using var testContext = new BunitContext();
        ConfigureServices(testContext.Services);

        _dropzoneTests.ShouldIndicateDropzoneAndRegisterItselfAsSuchOnDragStart(testContext,
            modifierCssClass: "highlighted",
            configureComponentParameters: ConfigureComponentParameters);
    }

    [Fact]
    public async Task Assert_indicate_drag_entered()
    {
        await using var testContext = new BunitContext();
        ConfigureServices(testContext.Services);

        await _dropzoneTests.ShouldIndicateDragEnteredAsync(testContext,
            configureComponentParameters: ConfigureComponentParameters);
    }

    [Fact]
    public async Task Assert_not_indicate_drag_entered_after_drag_leave()
    {
        await using var testContext = new BunitContext();
        ConfigureServices(testContext.Services);

        await _dropzoneTests.ShouldNotIndicateDragEnteredAfterDragLeaveAsync(testContext,
            configureComponentParameters: ConfigureComponentParameters);
    }

    [Fact]
    public async Task Assert_drag_dropped_handling()
    {
        await using var testContext = new BunitContext();
        ConfigureServices(testContext.Services);

        await _dropzoneTests.AssertDragDroppedHandlingAsync(testContext,
            configureComponentParameters: ConfigureComponentParameters);
    }

    [Fact]
    public async Task Assert_not_indicate_dropzone_and_drag_entered_after_drag_end()
    {
        await using var testContext = new BunitContext();
        ConfigureServices(testContext.Services);

        await _dropzoneTests.ShouldNotIndicateDropzoneAndDragEnteredAfterDragEndAsync(testContext,
            indicateDropZoneModifierCssClass: "highlighted",
            configureComponentParameters: ConfigureComponentParameters);
    }
}
