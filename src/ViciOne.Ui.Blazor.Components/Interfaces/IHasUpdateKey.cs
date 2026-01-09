namespace ViciOne.Ui.Blazor.Components.Interfaces;

/// <summary>
/// Describes a component that implements a <see cref="UpdateKey"/> parameter
/// </summary>
public interface IHasUpdateKey
{
    /// <summary>
    /// When set, the component will compare the update key with the previous update key to update itself on change.
    /// </summary>
    /// <remarks>
    /// This can be used to force an update of the component when other parameters did not change.
    /// </remarks>
    object? UpdateKey { get; set; }
}
