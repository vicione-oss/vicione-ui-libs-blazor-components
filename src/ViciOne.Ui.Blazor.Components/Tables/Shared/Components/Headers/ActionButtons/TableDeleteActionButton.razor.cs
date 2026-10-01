using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Localization.Resources;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Components.Headers.ActionButtons;

/// <summary>
/// Component for rendering a delete button in render fragment <see cref="TableHeader.ActionButtons"/>
/// </summary>
public sealed partial class TableDeleteActionButton : ComponentBase
{
    private readonly string _iconCssClass =
        MonochromeIconName.Delete.GetCssClasses(MonochromeIconSize.SmallMedium).ToSpaceSeparated();

    private string? _title;

    /// <inheritdoc cref="TableActionButton.CssClass"/>
    [Parameter]
    public string? CssClass { get; set; }

    /// <inheritdoc cref="TableActionButton.Enabled"/>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <inheritdoc cref="TableActionButton.Title"/>
    [Parameter]
    public string? Title { get; set; }

    /// <inheritdoc cref="TableActionButton.OnClick"/>
    [Parameter]
    public EventCallback OnClick { get; set; }

    private async Task ButtonClickAsync()
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync();
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
        => _title = Title ?? CommonVocabulary.Delete;
}
