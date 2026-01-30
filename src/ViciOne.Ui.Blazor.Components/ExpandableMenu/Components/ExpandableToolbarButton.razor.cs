using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.ExpandableMenu.Models;
using ViciOne.Ui.Blazor.Components.ExpandableMenu.Services;

namespace ViciOne.Ui.Blazor.Components.ExpandableMenu.Components;

/// <summary>
/// Button that on click makes expandable menu expanded or compact
/// </summary>
public sealed partial class ExpandableToolbarButton : ComponentBase, IDisposable
{
    [CascadingParameter]
    private ExpandableMenuEntry Entry { get; set; } = default!;

    [Inject]
    private ExpandableMenuService MenuService { get; set; } = default!;

    /// <inheritdoc/>
    public void Dispose()
    {
        MenuService.CompactChanged -= OnCompactChanged;
        MenuService.EntryExpansionChanged -= OnEntryExpansionChanged;
    }

    private async Task OnClickAsync()
    {
        if (!Entry.IsExpanded)
            Entry.IsExpanded = true;
        else if (Entry.IsSticky && !MenuService.IsCompact)
            Entry.IsExpanded = false;

        await MenuService.InvokeEntryExpansionChanged(Entry);

        if (MenuService.IsCompact)
            await MenuService.SetIsCompact(false);
    }

    private Task OnCollapseButtonClicked()
    {
        if (!MenuService.IsCompact)
            return MenuService.SetIsCompact(true);

        return Task.CompletedTask;
    }

    private Task OnCompactChanged()
        => InvokeAsync(StateHasChanged);

    private Task OnEntryExpansionChanged(ExpandableMenuEntry entry)
    {
        if (Entry == entry)
            return Task.CompletedTask;

        if ((!Entry.IsSticky && !entry.IsSticky) || (Entry.IsSticky && entry.IsSticky))
            Entry.IsExpanded = false;

        return InvokeAsync(StateHasChanged);
    }

    /// <inheritdoc/>
    protected override Task OnInitializedAsync()
    {
        MenuService.CompactChanged += OnCompactChanged;
        MenuService.EntryExpansionChanged += OnEntryExpansionChanged;

        if (Entry.IsDefault)
        {
            Entry.IsExpanded = true;
            return MenuService.InvokeEntryExpansionChanged(Entry);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
        => ArgumentNullException.ThrowIfNull(Entry, nameof(Entry));
}
