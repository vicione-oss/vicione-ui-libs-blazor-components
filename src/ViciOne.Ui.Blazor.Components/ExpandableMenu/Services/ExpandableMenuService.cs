using ViciOne.Ui.Blazor.Components.ExpandableMenu.Models;

namespace ViciOne.Ui.Blazor.Components.ExpandableMenu.Services;

internal sealed class ExpandableMenuService
{
    public ExpandableMenuEntry? CurrentRegularEntry { get; private set; }
    public ExpandableMenuEntry? CurrentStickyEntry { get; private set; }
    public bool IsCompact { get; private set; }

    public event Func<Task>? CompactChanged;
    public event Func<ExpandableMenuEntry, Task>? EntryExpansionChanged;

    public Task InvokeEntryExpansionChanged(ExpandableMenuEntry entry)
    {
        if (entry.IsSticky)
        {
            if (CurrentStickyEntry == entry && !IsCompact)
                CurrentStickyEntry = null;
            else
                CurrentStickyEntry = entry;
        }
        else
        {
            CurrentRegularEntry = entry;
        }

        return EntryExpansionChanged?.Invoke(entry) ?? Task.CompletedTask;
    }

    public Task SetIsCompact(bool isCompact)
    {
        if (IsCompact == isCompact)
            return Task.CompletedTask;

        IsCompact = isCompact;
        return CompactChanged?.Invoke() ?? Task.CompletedTask;
    }
}
