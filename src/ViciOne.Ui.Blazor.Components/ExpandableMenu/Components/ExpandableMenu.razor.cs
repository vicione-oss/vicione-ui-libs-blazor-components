using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Timers;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.ExpandableMenu.Models;
using ViciOne.Ui.Blazor.Components.ExpandableMenu.Services;

namespace ViciOne.Ui.Blazor.Components.ExpandableMenu.Components;

/// <summary>
/// Component for rendering an expandable menu
/// </summary>
public sealed partial class ExpandableMenu : ComponentBase, IDisposable
{
    private readonly System.Timers.Timer _entriesChangedTimer = new()
    {
        AutoReset = false,
        Interval = 20,
    };
    private bool _shouldRender;

    [Inject]
    private ExpandableMenuService MenuService { get; set; } = default!;

    /// <summary>
    /// True when compact mode is active, otherwise false.
    /// </summary>
    [Parameter]
    public bool CompactMode { get; set; }

    /// <summary>
    /// Notifies the consumer that the compact mode has changed.
    /// <para>
    /// The callback delivers a boolean value indicating the current
    /// compact mode state:<br />
    /// true = compact mode active <br />
    /// false = compact mode inactive <br />
    /// </para>
    /// </summary>
    [Parameter]
    public EventCallback<bool> CompactModeChanged { get; set; }

    /// <summary>
    /// A <see cref="ObservableCollection{T}"/> with <see cref="ExpandableMenuEntry"/> entries
    /// that should be shown in the expandable menu.
    /// </summary>
    [Parameter, EditorRequired]
    public ObservableCollection<ExpandableMenuEntry> Entries { get; init; } = [];

    /// <inheritdoc/>
    public void Dispose()
    {
        _entriesChangedTimer.Elapsed -= OnEntriesChangedTimerElapsedAsync;
        _entriesChangedTimer.Dispose();
        Entries.CollectionChanged -= OnEntriesChanged;
        MenuService.CompactChanged -= OnCompactChangedAsync;
        MenuService.EntryExpansionChanged -= OnEntryExpansionChanged;
    }

    private string GetAreaRowsDefinition()
        => $"{GetRegularAreaRowsDefinition()} {GetStickyAreaRowsDefinition()}";

    private string GetRegularAreaRowsDefinition()
    {
        var regularEntries = Entries.Where(e => !e.IsSticky).ToArray();

        if (regularEntries.Length == 0)
            return "auto";

        if (MenuService.CurrentRegularEntry is null || MenuService.IsCompact)
            return $"repeat({regularEntries.Length}, min-content) auto";

        var topCount = 0;
        var bottomCount = 0;
        foreach (var entry in regularEntries)
        {
            topCount++;

            if (MenuService.CurrentRegularEntry == entry)
            {
                bottomCount = regularEntries.Length - topCount;
                break;
            }
        }

        return $"repeat({topCount}, min-content) minmax(0, 100%)" +
               (bottomCount > 0 ? $" repeat({bottomCount}, min-content)" : string.Empty);
    }

    private string GetStickyAreaRowsDefinition()
    {
        var stickyEntries = Entries.Where(e => e.IsSticky).ToArray();

        if (stickyEntries.Length == 0)
            return string.Empty;

        if (MenuService.CurrentStickyEntry is null || MenuService.IsCompact)
            return $"repeat({stickyEntries.Length}, min-content)";

        var topCount = 0;
        var bottomCount = 0;
        foreach (var entry in stickyEntries)
        {
            topCount++;

            if (MenuService.CurrentStickyEntry == entry)
            {
                bottomCount = stickyEntries.Length - topCount;
                break;
            }
        }

        return $"repeat({topCount}, min-content) fit-content(50%)" +
               (bottomCount > 0 ? $" repeat({bottomCount}, min-content)" : string.Empty);
    }

    private async Task OnCompactChangedAsync()
    {
        if (CompactModeChanged.HasDelegate)
            await CompactModeChanged.InvokeAsync(MenuService.IsCompact);

        await Refresh();
    }

    private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => _entriesChangedTimer.Start();

    private async void OnEntriesChangedTimerElapsedAsync(object? sender, ElapsedEventArgs e)
        => await Refresh();

    private Task OnEntryExpansionChanged(ExpandableMenuEntry arg)
        => Refresh();

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _entriesChangedTimer.Elapsed += OnEntriesChangedTimerElapsedAsync;
        Entries.CollectionChanged += OnEntriesChanged;

        MenuService.CompactChanged += OnCompactChangedAsync;
        MenuService.SetIsCompact(CompactMode);

        MenuService.EntryExpansionChanged += OnEntryExpansionChanged;
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (CompactMode != MenuService.IsCompact)
            MenuService.SetIsCompact(CompactMode);

        _shouldRender = true;
    }

    private Task Refresh()
    {
        _shouldRender = true;
        return InvokeAsync(StateHasChanged);
    }

    /// <inheritdoc/>
    protected override bool ShouldRender()
    {
        if (_shouldRender)
        {
            _shouldRender = false;
            return true;
        }

        return false;
    }
}
