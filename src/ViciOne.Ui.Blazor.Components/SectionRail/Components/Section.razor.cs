using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.SectionRail.Components;

/// <inheritdoc cref="ISection{TSectionIdentifier}"/>
public sealed partial class Section<TSectionIdentifier> : ComponentBase, ISection<TSectionIdentifier>, IDisposable
{
    [Inject]
    private IEqualityComparer<TSectionIdentifier> SectionIdentifierEqualityComparer { get; set; } = default!;

    /// <inheritdoc/>
    [CascadingParameter]
    public required ISectionRail<TSectionIdentifier> SectionRail { get; set; }

    /// <inheritdoc/>
    [Parameter, EditorRequired]
    public required TSectionIdentifier Id { get; set; }

    /// <inheritdoc/>
    [Parameter, EditorRequired]
    public required string Title { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public string? IconCssClass { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public Uri? IconUrl { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public string? IconData { get; set; }

    /// <summary>
    /// Content rendered into the section
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        if (SectionRail != null)
        {
            SectionRail.RegisterItem(this);
        }
        else
        {
            const string Section = nameof(Section<>);
            var sectionIdentifier = typeof(TSectionIdentifier).Name;
            const string SectionRail = nameof(ISectionRail<>);

            throw new ArgumentNullException(nameof(Section<>.SectionRail),
                $"{Section}<{sectionIdentifier}> must exist within a {SectionRail}<{sectionIdentifier}>.");
        }
    }

    /// <inheritdoc/>
    public void Dispose()
        => SectionRail?.UnregisterItem(this);
}
