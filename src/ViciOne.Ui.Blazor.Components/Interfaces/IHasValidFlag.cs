namespace ViciOne.Ui.Blazor.Components.Interfaces;

/// <summary>
/// Describes a component that implements a <see cref="Valid"/> parameter
/// </summary>
public interface IHasValidFlag
{
    /// <summary>
    /// Optionally specifies whether the input is valid or not
    /// </summary>
    bool? Valid { get; set; }
}
