namespace ViciOne.Ui.Blazor.Components.SectionRail.Components;

/// <summary>
/// Section rail
/// </summary>
public interface ISectionRail<TSectionIdentifier>
{
    /// <summary>
    /// Identifier of the active section
    /// </summary>
    TSectionIdentifier ActiveSectionId { get; }

    /// <summary>
    /// True when sections should be rendered, otherwise false
    /// </summary>
    bool Expanded { get; }

    /// <summary>
    /// Register the given <paramref name="section"/> with the section rail
    /// </summary>
    void RegisterItem(ISection<TSectionIdentifier> section);

    /// <summary>
    /// Unregisters the given <paramref name="section"/> from the section rail
    /// </summary>
    void UnregisterItem(ISection<TSectionIdentifier> section);
}
