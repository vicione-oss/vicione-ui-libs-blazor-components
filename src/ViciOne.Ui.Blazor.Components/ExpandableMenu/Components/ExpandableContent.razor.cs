using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.ExpandableMenu.Models;
using ViciOne.Ui.Blazor.Components.ExpandableMenu.Services;

namespace ViciOne.Ui.Blazor.Components.ExpandableMenu.Components;

/// <summary>
/// Renders content specified by <see cref="ExpandableMenuEntry.ContentType"/>
/// </summary>
public sealed partial class ExpandableContent : ComponentBase, IDisposable
{
    [CascadingParameter]
    private ExpandableMenuEntry Entry { get; set; } = default!;

    [Inject]
    private ExpandableMenuService MenuService { get; set; } = default!;

    /// <inheritdoc/>
    public void Dispose()
        => MenuService.EntryExpansionChanged -= MenuServiceEntryExpansionChangedAsync;

    private Task MenuServiceEntryExpansionChangedAsync(ExpandableMenuEntry entry)
        => InvokeAsync(StateHasChanged);

    /// <inheritdoc/>
    protected override void OnInitialized()
        => MenuService.EntryExpansionChanged += MenuServiceEntryExpansionChangedAsync;

    /// <inheritdoc/>
    protected override void OnParametersSet()
        => ArgumentNullException.ThrowIfNull(Entry, nameof(Entry));
}
