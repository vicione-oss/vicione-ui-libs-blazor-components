using Microsoft.AspNetCore.Components.Web;
using Server.Services;

namespace Server.Tests.Services;

public class RenderModeProviderTests
{
    [Fact]
    public void Content_render_mode_should_be_server_by_default()
    {
        // Arrange
        var provider = new RenderModeProvider();

        // Act
        var result = provider.ContentRenderMode;

        // Assert
        result.Should().BeOfType<InteractiveServerRenderMode>();
    }

    [Fact]
    public void Header_render_mode_should_be_server_by_default()
    {
        // Arrange
        var provider = new RenderModeProvider();

        // Act
        var result = provider.HeaderRenderMode;

        // Assert
        result.Should().BeOfType<InteractiveServerRenderMode>();
    }

    [Fact]
    public void Use_webassembly_should_be_false_by_default()
    {
        // Arrange
        var provider = new RenderModeProvider();

        // Act
        var result = provider.UseWebassembly;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Content_render_mode_should_be_wasm_when_use_wasm_is_true()
    {
        // Arrange
        var provider = new RenderModeProvider(useWasm: true);

        // Act
        var result = provider.ContentRenderMode;

        // Assert
        result.Should().BeOfType<InteractiveWebAssemblyRenderMode>();
    }

    [Fact]
    public void Header_render_mode_should_be_wasm_when_use_wasm_is_true()
    {
        // Arrange
        var provider = new RenderModeProvider(useWasm: true);

        // Act
        var result = provider.HeaderRenderMode;

        // Assert
        result.Should().BeOfType<InteractiveWebAssemblyRenderMode>();
    }

    [Fact]
    public void Use_webassembly_should_be_true_when_use_wasm_is_true()
    {
        // Arrange
        var provider = new RenderModeProvider(useWasm: true);

        // Act
        var result = provider.UseWebassembly;

        // Assert
        result.Should().BeTrue();
    }
}
