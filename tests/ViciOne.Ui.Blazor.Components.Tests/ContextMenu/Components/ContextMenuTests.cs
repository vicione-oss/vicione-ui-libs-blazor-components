using Bunit;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Extensions;
using ContextMenuComponent = ViciOne.Ui.Blazor.Components.ContextMenu.Components.ContextMenu;

namespace ViciOne.Ui.Blazor.Components.Tests.ContextMenu.Components;

public sealed class ContextMenuTests
{
    private static readonly string s_jsModulePath =
        $"./_content/{typeof(ContextMenuComponent).Assembly.GetName().Name}/context-menu/components/context-menu.js";

    [Fact]
    public async Task Should_dispose_module_without_creating_script_when_disposed_while_module_is_loading()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddPopup();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var pendingImport = PendingJSModuleImport.SetUp(testContext, s_jsModulePath);

        var renderedComponent = testContext.Render<ContextMenuComponent>();

        // The module is loaded the first time the menu is shown.
        await renderedComponent.Instance.ShowAsync(new MouseEventArgs(), null);
        await pendingImport.WaitUntilRequestedAsync();

        // Act
        await testContext.DisposeComponentsAsync();
        await pendingImport.CompleteAsync();

        // Assert
        await pendingImport.Module.Received(1).DisposeAsync();
        await pendingImport.Module.DidNotReceive()
            .InvokeConstructorAsync(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<object?[]?>());
    }

    [Fact]
    public async Task Should_become_visible_once_its_position_is_calculated()
    {
        // Arrange
        await using var testContext = new BunitContext();
        var calculatePosition = SetUpCalculatePosition(testContext);

        var visibilityChanges = new List<bool>();
        var renderedComponent = testContext.Render<ContextMenuComponent>(b => b
            .Add(p => p.VisibilityChanged, visibilityChanges.Add));

        await renderedComponent.Instance.ShowAsync(new MouseEventArgs(), null);
        renderedComponent.WaitForAssertion(() => calculatePosition.Invocations.Should().ContainSingle());

        // Act
        await renderedComponent.InvokeAsync(() => calculatePosition.SetResult(new ContextMenuPosition(1, 2)));
        await renderedComponent.InvokeAsync(() => { });

        // Assert
        renderedComponent.Instance.Visible.Should().BeTrue();
        visibilityChanges.Should().Equal(true);
    }

    [Fact]
    public async Task Should_stay_hidden_when_closed_while_its_position_is_calculated()
    {
        // Arrange
        await using var testContext = new BunitContext();
        var calculatePosition = SetUpCalculatePosition(testContext);

        var visibilityChanges = new List<bool>();
        var renderedComponent = testContext.Render<ContextMenuComponent>(b => b
            .Add(p => p.VisibilityChanged, visibilityChanges.Add));

        await renderedComponent.Instance.ShowAsync(new MouseEventArgs(), null);
        renderedComponent.WaitForAssertion(() => calculatePosition.Invocations.Should().ContainSingle());

        // Act
        await renderedComponent.Instance.CloseAsync();
        await renderedComponent.InvokeAsync(() => calculatePosition.SetResult(new ContextMenuPosition(1, 2)));
        await renderedComponent.InvokeAsync(() => { });

        // Assert
        renderedComponent.Instance.Visible.Should().BeFalse();
        visibilityChanges.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_report_visible_only_once_shown_on_screen()
    {
        // Arrange
        await using var testContext = new BunitContext();
        var jsInstance = SetUpScript(testContext);
        jsInstance.Setup<ContextMenuPosition>("calculatePosition", _ => true).SetResult(new ContextMenuPosition(1, 2));
        var startObserve = jsInstance.SetupVoid("startObserveWindowPointerDown", _ => true);

        var visibilityChanges = new List<bool>();
        var renderedComponent = testContext.Render<ContextMenuComponent>(b => b
            .Add(p => p.VisibilityChanged, visibilityChanges.Add));

        await renderedComponent.Instance.ShowAsync(new MouseEventArgs(), null);
        renderedComponent.WaitForAssertion(() => startObserve.Invocations.Should().ContainSingle());
        var visibilityChangesBeforeShown = visibilityChanges.ToList();

        // Act
        await renderedComponent.InvokeAsync(startObserve.SetVoidResult);
        await renderedComponent.InvokeAsync(() => { });

        // Assert
        visibilityChangesBeforeShown.Should().BeEmpty();
        visibilityChanges.Should().Equal(true);
    }

    [Fact]
    public async Task Should_report_nothing_when_closed_while_click_outside_observation_starts()
    {
        // Arrange
        await using var testContext = new BunitContext();
        var jsInstance = SetUpScript(testContext);
        jsInstance.Setup<ContextMenuPosition>("calculatePosition", _ => true).SetResult(new ContextMenuPosition(1, 2));
        var startObserve = jsInstance.SetupVoid("startObserveWindowPointerDown", _ => true);
        var endObserve = jsInstance.SetupVoid("endObserveWindowPointerDown", _ => true);
        endObserve.SetVoidResult();

        var visibilityChanges = new List<bool>();
        var renderedComponent = testContext.Render<ContextMenuComponent>(b => b
            .Add(p => p.VisibilityChanged, visibilityChanges.Add));

        await renderedComponent.Instance.ShowAsync(new MouseEventArgs(), null);
        renderedComponent.WaitForAssertion(() => startObserve.Invocations.Should().ContainSingle());

        // Act
        await renderedComponent.Instance.CloseAsync();
        await renderedComponent.InvokeAsync(startObserve.SetVoidResult);
        await renderedComponent.InvokeAsync(() => { });

        // Assert
        renderedComponent.Instance.Visible.Should().BeFalse();
        visibilityChanges.Should().BeEmpty();
        endObserve.Invocations.Should().ContainSingle();
    }

    [Fact]
    public async Task Should_report_hidden_after_visible_when_closed_while_visibility_change_is_reported()
    {
        // Arrange
        await using var testContext = new BunitContext();
        var jsInstance = SetUpScript(testContext);
        jsInstance.Setup<ContextMenuPosition>("calculatePosition", _ => true).SetResult(new ContextMenuPosition(1, 2));
        jsInstance.SetupVoid("startObserveWindowPointerDown", _ => true).SetVoidResult();
        var endObserve = jsInstance.SetupVoid("endObserveWindowPointerDown", _ => true);
        endObserve.SetVoidResult();

        var visibilityChanges = new List<bool>();
        IRenderedComponent<ContextMenuComponent>? renderedComponent = null;

        // Act
        renderedComponent = testContext.Render<ContextMenuComponent>(b => b
            .Add(p => p.VisibilityChanged, async visible =>
            {
                visibilityChanges.Add(visible);

                if (visible)
                    await renderedComponent!.Instance.CloseAsync();
            }));

        await renderedComponent.Instance.ShowAsync(new MouseEventArgs(), null);
        renderedComponent.WaitForAssertion(() => visibilityChanges.Should().Equal(true, false));
        await renderedComponent.InvokeAsync(() => { });

        // Assert
        renderedComponent.Instance.Visible.Should().BeFalse();
        endObserve.Invocations.Should().ContainSingle();
    }

    [Fact]
    public async Task Should_close_when_pointer_is_pressed_outside()
    {
        // Arrange
        await using var testContext = new BunitContext();
        var renderedComponent = await RenderVisibleAsync(testContext);

        var visibleContextMenus = await renderedComponent.InvokeAsync(renderedComponent.Instance.GetVisibleContextMenus);

        // Act
        await renderedComponent.Instance.CloseOnPointerDownOutsideAsync(visibleContextMenus.RequestVersion);
        await renderedComponent.InvokeAsync(() => { });

        // Assert
        renderedComponent.Instance.Visible.Should().BeFalse();
    }

    [Fact]
    public async Task Should_stay_visible_when_shown_again_before_pointer_pressed_outside_closes_it()
    {
        // Arrange
        await using var testContext = new BunitContext();
        var renderedComponent = await RenderVisibleAsync(testContext);

        var visibleContextMenus = await renderedComponent.InvokeAsync(renderedComponent.Instance.GetVisibleContextMenus);

        // Act
        await renderedComponent.Instance.ShowAsync(new MouseEventArgs(), null);
        await renderedComponent.Instance.CloseOnPointerDownOutsideAsync(visibleContextMenus.RequestVersion);
        await renderedComponent.InvokeAsync(() => { });

        // Assert
        renderedComponent.Instance.Visible.Should().BeTrue();
    }

    /// <summary>
    /// Renders the context menu with a script answering every call right away, and shows it
    /// </summary>
    private static async Task<IRenderedComponent<ContextMenuComponent>> RenderVisibleAsync(BunitContext testContext)
    {
        var jsInstance = SetUpScript(testContext);
        jsInstance.Setup<ContextMenuPosition>("calculatePosition", _ => true).SetResult(new ContextMenuPosition(1, 2));
        jsInstance.SetupVoid("startObserveWindowPointerDown", _ => true).SetVoidResult();
        jsInstance.SetupVoid("endObserveWindowPointerDown", _ => true).SetVoidResult();

        var renderedComponent = testContext.Render<ContextMenuComponent>();

        await renderedComponent.Instance.ShowAsync(new MouseEventArgs(), null);
        renderedComponent.WaitForAssertion(() => renderedComponent.Instance.Visible.Should().BeTrue());

        return renderedComponent;
    }

    /// <summary>
    /// Holds the position calculated in the browser pending until the test sets it
    /// </summary>
    private static JSRuntimeInvocationHandler<ContextMenuPosition> SetUpCalculatePosition(BunitContext testContext)
        => SetUpScript(testContext).Setup<ContextMenuPosition>("calculatePosition", _ => true);

    /// <summary>
    /// Sets up the script of the context menu, answering every call not set up otherwise with a default result
    /// </summary>
    private static BunitJSModuleInterop SetUpScript(BunitContext testContext)
    {
        testContext.Services.AddPopup();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        return testContext.JSInterop.SetupModule(s_jsModulePath).SetupModule("ContextMenu", _ => true);
    }
}
