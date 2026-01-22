using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Server.Services;

public class RenderModeProvider(bool useWasm = false)
{
    public IComponentRenderMode ContentRenderMode { get; }
        = useWasm ? new InteractiveWebAssemblyRenderMode(prerender: false) : new InteractiveServerRenderMode();

    public IComponentRenderMode HeaderRenderMode { get; }
        = useWasm ? new InteractiveWebAssemblyRenderMode(prerender: false) : new InteractiveServerRenderMode();

    public bool UseWebassembly { get; } = useWasm;
}
