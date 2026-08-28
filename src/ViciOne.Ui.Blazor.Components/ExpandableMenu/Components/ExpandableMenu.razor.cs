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
        _entriesChangedTimer.Elapsed -= EntriesChangedTimerElapsedAsync;
        _entriesChangedTimer.Dispose();
        Entries.CollectionChanged -= EntriesCollectionChanged;
        MenuService.CompactChanged -= MenuServiceCompactChangedAsync;
        MenuService.EntryExpansionChanged -= MenuServiceEntryExpansionChangedAsync;
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

        var bottomRowsDefinition = bottomCount > 0 ? $" repeat({bottomCount}, min-content)" : string.Empty;

        return $"repeat({topCount}, min-content) minmax(0, 100%)" + bottomRowsDefinition;
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

        var bottomRowsDefinition = bottomCount > 0 ? $" repeat({bottomCount}, min-content)" : string.Empty;

        return $"repeat({topCount}, min-content) fit-content(50%)" + bottomRowsDefinition;
    }

    private async Task MenuServiceCompactChangedAsync()
    {
        if (CompactModeChanged.HasDelegate)
            await CompactModeChanged.InvokeAsync(MenuService.IsCompact);

        await RefreshAsync();
    }

    private void EntriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => _entriesChangedTimer.Start();

    private async void EntriesChangedTimerElapsedAsync(object? sender, ElapsedEventArgs e)
        => await RefreshAsync();

    private Task MenuServiceEntryExpansionChangedAsync(ExpandableMenuEntry arg)
        => RefreshAsync();

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _entriesChangedTimer.Elapsed += EntriesChangedTimerElapsedAsync;
        Entries.CollectionChanged += EntriesCollectionChanged;

        MenuService.CompactChanged += MenuServiceCompactChangedAsync;
        MenuService.SetIsCompactAsync(CompactMode);

        MenuService.EntryExpansionChanged += MenuServiceEntryExpansionChangedAsync;
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (CompactMode != MenuService.IsCompact)
            MenuService.SetIsCompactAsync(CompactMode);

        _shouldRender = true;
    }

    private Task RefreshAsync()
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
