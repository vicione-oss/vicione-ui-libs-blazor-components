using Server.Extensions;
using Server.Services;
using Shared.Extensions;
using ViciOne.Ui.MonochromeIcons.Assets.Extensions;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "wwwroot",
});

var useWebAssembly = builder.Configuration.GetValue<bool>("RenderWasm");

builder.Services.AddAntiforgery();

if (useWebAssembly)
{
    builder.Services
        .AddUrlBasedMonochromeIconSvgMarkupProvider()
        .AddRazorComponents()
        .AddInteractiveWebAssemblyComponents();
}
else
{
    builder.Services
        .AddFileSystemBasedMonochromeIconSvgMarkupProvider<Program>()
        .AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddHubOptions(configure => configure.MaximumReceiveMessageSize = 5 * 1024 * 1024);
}

builder.Services.AddHttpClient();
builder.Services.AddLocalization();

builder.WebHost.UseStaticWebAssets();

builder.Services.AddSingleton(new RenderModeProvider(useWebAssembly));
builder.Services.AddShared();

var app = builder.Build();

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.UseRenderMode(useWebAssembly);

await app.RunAsync();
