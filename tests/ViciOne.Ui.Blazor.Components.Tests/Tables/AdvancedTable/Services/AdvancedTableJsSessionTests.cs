using Bunit;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Services;
using ViciOne.Ui.Blazor.Components.Tests.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Services;

public sealed class AdvancedTableJsSessionTests
{
    private static readonly string s_jsModulePath =
        $"./_content/{typeof(AdvancedTableJsSession<>).Assembly.GetName().Name}/advanced-table/components/advanced-table.js";

    [Fact]
    public async Task Should_dispose_module_without_attaching_when_disposed_while_module_is_loading()
    {
        // Arrange
        await using var testContext = new BunitContext();

        var pendingImport = new PendingJSModuleImport(testContext.JSInterop.JSRuntime, s_jsModulePath);

        await using var session = new AdvancedTableJsSession<object>(pendingImport, new object());

        var attach = session.AttachAsync(default);
        await pendingImport.WaitUntilRequestedAsync();

        // Act
        await session.DisposeAsync();
        await pendingImport.CompleteAsync();
        await attach;

        // Assert
        await pendingImport.Module.Received(1).DisposeAsync();
        await pendingImport.Module.DidNotReceive()
            .InvokeAsync<IJSObjectReference>(Arg.Any<string>(), Arg.Any<object?[]?>());
        session.IsAttached.Should().BeFalse();
    }
}
