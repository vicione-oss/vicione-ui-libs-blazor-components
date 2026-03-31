using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ButtonComponent = ViciOne.Ui.Blazor.Components.Button.Button;

namespace ViciOne.Ui.Blazor.Components.Dialog.Components;

/// <summary>
/// Renders a button for use in <see cref="Dialog.Footer"/>.
/// </summary>
public sealed partial class DialogFooterButton : ComponentBase
{
    /// <inheritdoc cref="ButtonComponent.Id"/>
    [Parameter] public string? Id { get; set; }

    /// <inheritdoc cref="ButtonComponent.CssClass"/>
    [Parameter] public string? CssClass { get; set; }

    /// <inheritdoc/>
    [Parameter] public MonochromeIconName? IconName { get; set; }

    /// <inheritdoc cref="ButtonComponent.Text"/>
    [Parameter, Required] public required string Text { get; set; }

    /// <inheritdoc cref="ButtonComponent.Title"/>
    [Parameter] public string? Title { get; set; }

    /// <inheritdoc cref="ButtonComponent.Enabled"/>
    [Parameter] public bool Enabled { get; set; } = true;

    /// <inheritdoc cref="ButtonComponent.OnClick"/>
    [Parameter] public EventCallback OnClick { get; set; }

    private async Task ButtonClickAsync()
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync();
    }
}
