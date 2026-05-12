namespace ViciOne.Ui.Blazor.Components.SectionRail.Components;

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
