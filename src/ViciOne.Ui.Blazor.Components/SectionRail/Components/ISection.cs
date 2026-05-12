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
