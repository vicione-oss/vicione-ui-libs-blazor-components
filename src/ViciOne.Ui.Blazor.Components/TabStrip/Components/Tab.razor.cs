using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.TabStrip.Models;

namespace ViciOne.Ui.Blazor.Components.TabStrip.Components;

/// <summary>
/// Represents a single tab inside a <see cref="Components.TabStrip"/> component.
/// </summary>
public sealed partial class Tab : ComponentBase, ITab, IDisposable
{
    private ElementReference _elementReference;

    /// <summary>
    /// Gets or sets the text displayed on the tab.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the tab is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<TabSelectedEventArgs> OnSelected { get; set; }

    [CascadingParameter]
    private ITabStrip TabStrip { get; set; } = default!;

    internal bool Active => TabStrip.GetTabIndex(this) == TabStrip.ActiveTabIndex;

    /// <inheritdoc/>
    protected override void OnInitialized()
        => TabStrip.AddTab(this);

    /// <inheritdoc/>
    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (firstRender)
            TabStrip.RegisterTabElement(this, _elementReference);
    }

    /// <inheritdoc/>
    public void Dispose()
        => TabStrip.RemoveTab(this);

    private async Task SelectAsync()
    {
        var index = TabStrip.GetTabIndex(this);

        if (index == TabStrip.ActiveTabIndex)
            return;

        var currentActiveTabIndex = TabStrip.ActiveTabIndex;
        var newActiveTabIndex = index;

        await OnSelected.InvokeAsync(new TabSelectedEventArgs(newActiveTabIndex));

        await TabStrip.ChangeActiveTabIndexAsync(currentActiveTabIndex, newActiveTabIndex);
    }

    private Task OnKeyDownAsync(KeyboardEventArgs args)
    {
        if (args.IsEnter())
            return SelectAsync();

        return Task.CompletedTask;
    }

    Task ITab.RenderAsync()
        => InvokeAsync(StateHasChanged);
}
