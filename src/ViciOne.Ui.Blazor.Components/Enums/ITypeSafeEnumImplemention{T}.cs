namespace ViciOne.Ui.Blazor.Components.Enums;

/// <summary>
/// Type-safe enum contract represented by <typeparamref name="T"/>.
/// Implementations typically expose a fixed set of named instances and provide a default value.
/// </summary>
/// <typeparam name="T">
/// The concrete value type that implements this interface.
/// </typeparam>
public interface ITypeSafeEnumImplemention<T>
{
    /// <summary>
    /// Gets the default instance for this type-safe enum.
    /// </summary>
    /// <returns>
    /// The default <typeparamref name="T"/> instance.
    /// </returns>
    static abstract T GetDefaultValue();

    /// <summary>
    /// Gets the logical name of the current instance.
    /// </summary>
    /// <returns>
    /// The name of this value.
    /// </returns>
    string GetName();
}
