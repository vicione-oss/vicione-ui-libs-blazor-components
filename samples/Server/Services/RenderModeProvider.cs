using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Server.Services;

internal sealed class RenderModeProvider(bool useWasm = false)
{
    public IComponentRenderMode ContentRenderMode { get; } = CreateRenderMode(useWasm);

    public IComponentRenderMode HeaderRenderMode { get; } = CreateRenderMode(useWasm);

    public bool UseWebassembly { get; } = useWasm;

    private static IComponentRenderMode CreateRenderMode(bool useWasm)
    {
        if (useWasm)
            return new InteractiveWebAssemblyRenderMode(prerender: false);

        return new InteractiveServerRenderMode();
    }
}
