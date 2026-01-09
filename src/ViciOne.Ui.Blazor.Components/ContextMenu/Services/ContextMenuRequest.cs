using ViciOne.Ui.Blazor.Components.ContextMenu.Models;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Services;

internal sealed class ContextMenuRequest<TContext>(IContextMenuSettings settings) : IContextMenuRequest<TContext>
     where TContext : IContextMenuContext
{
    public event Func<TContext, Task>? ContextMenuRequestedAsync;

    public async Task SendAsync(TContext context)
    {
        if (!settings.UseCustomMenu)
            return;

        if (ContextMenuRequestedAsync is not null)
            await ContextMenuRequestedAsync.Invoke(context);
    }
}
