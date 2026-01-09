using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Localization.Resources;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace ViciOne.Ui.Blazor.Components.Grid.Components.ActionButtons;

/// <summary>
/// Component for rendering an edit button in render fragment <see cref="Grid{TGridItem}.ActionButtons"/>
/// </summary>
public sealed partial class EditGridActionButton : ComponentBase
{
    private readonly string _iconCssClass =
        MonochromeIconName.Edit.GetCssClasses(MonochromeIconSize.SmallMedium).ToSpaceSeparated();

    private string? _title;

    /// <inheritdoc cref="GridActionButton.CssClass"/>
    [Parameter]
    public string? CssClass { get; set; }

    /// <inheritdoc cref="GridActionButton.Enabled"/>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <inheritdoc cref="GridActionButton.Title"/>
    [Parameter]
    public string? Title { get; set; }

    /// <inheritdoc cref="GridActionButton.OnClick"/>
    [Parameter]
    public EventCallback OnClick { get; set; }

    private async Task ButtonClickAsync()
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync();
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
        => _title = Title ?? CommonVocabulary.EditVerb;
}
