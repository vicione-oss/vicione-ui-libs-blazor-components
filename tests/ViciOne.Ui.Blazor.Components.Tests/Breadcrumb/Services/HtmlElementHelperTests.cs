using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.Testing.Platform.Services;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Services;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.Tests.Breadcrumb.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Breadcrumb.Services;

public sealed class HtmlElementHelperTests
{
    [Fact]
    public async Task Should_return_result_when_elements_exist()
    {
        // Arrange
        const int Width1 = 10;
        const int Width2 = 20;
        var size1 = new DomRect { Width = Width1, Height = 1 };
        var size2 = new DomRect { Width = Width2, Height = 1 };

        await using var testContext = new BunitContext();
        testContext.JSInterop.SetupGetBoundingClientRects().SetResult([size1, size2]);

        var jsRuntime = testContext.Services.GetRequiredService<IJSRuntime>();
        await using var htmlElementHelper = new HtmlElementHelper(jsRuntime, Substitute.For<ILogger<HtmlElementHelper>>());

        // Act
        var result = await htmlElementHelper.GetBoundingClientRectsAsync([new ElementReference("id")]);

        // Assert
        result.Should().HaveCount(2);
        result.Sum(r => r.Width).Should().Be(Width1 + Width2);
    }

    [Fact]
    public async Task Should_return_empty_when_element_exists_but_dispose_happened()
    {
        // Arrange
        var size1 = new DomRect { Width = 10, Height = 1 };
        var size2 = new DomRect { Width = 20, Height = 1 };

        await using var testContext = new BunitContext();
        testContext.JSInterop.SetupGetBoundingClientRects().SetResult([size1, size2]);

        var jsRuntime = testContext.Services.GetRequiredService<IJSRuntime>();
        await using var htmlElementHelper = new HtmlElementHelper(jsRuntime, Substitute.For<ILogger<HtmlElementHelper>>());

        // Act
        await htmlElementHelper.DisposeAsync();
        var result = await htmlElementHelper.GetBoundingClientRectsAsync([new ElementReference("id")]);

        // Assert
        result.Should().BeEmpty();
        testContext.JSInterop.VerifyNotInvoke("getBoundingClientRects");
    }

    [Fact]
    public async Task Should_return_empty_when_element_does_not_exist()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.JSInterop.SetupGetBoundingClientRects().SetResult([]);

        var jsRuntime = testContext.Services.GetRequiredService<IJSRuntime>();
        await using var htmlElementHelper = new HtmlElementHelper(jsRuntime, Substitute.For<ILogger<HtmlElementHelper>>());

        // Act
        var result = await htmlElementHelper.GetBoundingClientRectsAsync([new ElementReference("id")]);

        // Assert
        result.Should().BeEmpty();
        testContext.JSInterop.VerifyInvoke("getBoundingClientRects");
    }
}
