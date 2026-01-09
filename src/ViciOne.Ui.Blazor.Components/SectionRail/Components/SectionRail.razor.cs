using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.SectionRail.Components;

/// <inheritdoc cref="ISectionRail{TSectionUniqueIdentifier}"/>
public sealed partial class SectionRail<TSectionIdentifier> : ComponentBase, ISectionRail<TSectionIdentifier>
{
    private readonly List<ISection<TSectionIdentifier>> _sections = [];

    [Inject]
    private IEqualityComparer<TSectionIdentifier> SectionIdentifierEqualityComparer { get; set; } = default!;

    /// <summary>
    /// Render fragment for rendering <see cref="Section{TIdentifier}"/> components
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <inheritdoc/>
    [Parameter, EditorRequired]
    public required TSectionIdentifier ActiveSectionId { get; set; }

    /// <summary>
    /// Raised when <see cref="ActiveSectionId"/> has changed
    /// </summary>
    [Parameter]
    public EventCallback<TSectionIdentifier> ActiveSectionIdChanged { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public bool Expanded { get; set; }

    /// <summary>
    /// Raised when <see cref="Expanded"/> has changed
    /// </summary>
    [Parameter]
    public EventCallback<bool> ExpandedChanged { get; set; }

    /// <inheritdoc/>
    public void RegisterItem(ISection<TSectionIdentifier> section)
    {
        if (!_sections.Contains(section))
        {
            _sections.Add(section);

            InvokeAsync(StateHasChanged);
        }
    }

    /// <inheritdoc/>
    public void UnregisterItem(ISection<TSectionIdentifier> section)
    {
        if (_sections.Remove(section))
            InvokeAsync(StateHasChanged);
    }

    private async Task SectionButtonClickAsync(ISection<TSectionIdentifier> section)
    {
        if (SectionIdentifierEqualityComparer.Equals(section.Id, ActiveSectionId))
        {
            await ToggleExpandedAsync();
        }
        else
        {
            if (!Expanded)
                await ToggleExpandedAsync();

            if (ActiveSectionIdChanged.HasDelegate)
                await ActiveSectionIdChanged.InvokeAsync(section.Id);

            ActiveSectionId = section.Id;
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task ToggleExpandedAsync()
    {
        var newExpanded = !Expanded;

        if (ExpandedChanged.HasDelegate)
            await ExpandedChanged.InvokeAsync(newExpanded);

        Expanded = newExpanded;
    }
}
