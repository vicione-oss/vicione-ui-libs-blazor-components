using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.SectionRail.Components;

/// <summary>
/// Section
/// </summary>
public interface ISection : IHasIcon
{
    /// <summary>
    /// Title for the section
    /// </summary>
    string Title { get; }
}

/// <inheritdoc/>
public interface ISection<TSectionIdentifier> : ISection
{
    /// <summary>
    /// Identifier for the section
    /// </summary>
    TSectionIdentifier Id { get; }

    /// <summary>
    /// Section rail the section belongs to
    /// </summary>
    ISectionRail<TSectionIdentifier> SectionRail { get; }
}
