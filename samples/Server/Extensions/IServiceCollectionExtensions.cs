using Shared.Components;

namespace Server.Extensions;

public static class IServiceCollectionExtensions
{
    public static WebApplication UseRenderMode(this WebApplication app, bool useWebAssembly)
    {
        if (useWebAssembly)
        {
            app.UseWebAssemblyDebugging();

            app.MapStaticAssets();

            app.MapRazorComponents<App>()
                .AddAdditionalAssemblies([typeof(Routes).Assembly])
                .AddInteractiveWebAssemblyRenderMode();
        }
        else
        {
            app.MapRazorComponents<App>()
                .AddAdditionalAssemblies([typeof(Routes).Assembly])
                .AddInteractiveServerRenderMode();
        }

        return app;
    }
}
