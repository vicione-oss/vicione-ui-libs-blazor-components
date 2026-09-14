using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using ViciOne.Ui.Blazor.Components.Enums;

namespace ViciOne.Ui.Blazor.Components.Factories;

/// <summary>
/// Factory for creating instances of <typeparamref name="T"/> discovered via reflection
/// following the type-safe enum pattern.
/// </summary>
/// <typeparam name="T">
/// A struct that implements <see cref="IEquatable{T}"/> and
/// <see cref="ITypeSafeEnumImplemention{T}"/> and exposes public static fields
/// representing the defined values, plus the required members
/// (static abstract T GetDefaultValue(), string GetName()).
/// </typeparam>
/// <remarks>
/// <para>On type initialization, all public static fields of <typeparamref name="T"/> are scanned
/// and indexed by their GetName() value for fast lookup.</para>
/// <para>Name comparison uses the exact key as stored in the internal dictionary.</para>
/// </remarks>
public static class TypeSafeEnumFactory<T> where T : struct, IEquatable<T>, ITypeSafeEnumImplemention<T>
{
    private static readonly T s_defaultValue = T.GetDefaultValue();

    private static readonly Dictionary<string, T> s_typeSafeEnumImplementationMap = typeof(T)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(f => f.GetValue(null) as T?)
        .Where(i => i is not null)
        .Select(i => (T)i!)
        .ToDictionary(i => i.GetName());

    /// <summary>
    /// Returns an instance of <typeparamref name="T"/> for the given <paramref name="enumValueName"/>,
    /// or the default value if no match is found.
    /// </summary>
    /// <param name="enumValueName">The logical name as returned by T.GetName().</param>
    /// <returns>
    /// The matching instance if present; otherwise the default instance from T.GetDefaultValue().
    /// </returns>
    public static T Create(string enumValueName)
    {
        if (s_typeSafeEnumImplementationMap.TryGetValue(enumValueName, out var result))
            return result;
        else
            return s_defaultValue;
    }

    /// <summary>
    /// Tries to return an instance of <typeparamref name="T"/> for the given <paramref name="enumValueName"/>.
    /// </summary>
    /// <param name="enumValueName">The logical name as returned by T.GetName().</param>
    /// <param name="enumValue">
    /// When this method returns true, contains the matching instance; otherwise the default value.
    /// </param>
    /// <returns>
    /// true if a matching instance was found; otherwise false.
    /// </returns>
    public static bool TryCreate(string enumValueName, [MaybeNullWhen(false)] out T enumValue)
        => s_typeSafeEnumImplementationMap.TryGetValue(enumValueName, out enumValue);

    /// <summary>
    /// Returns all available instances of <typeparamref name="T"/> discovered on the type.
    /// </summary>
    /// <returns>
    /// A read-only collection of all instances, for example {Red, Green, Blue}.
    /// </returns>
    public static IReadOnlyCollection<T> CreateAll()
        => s_typeSafeEnumImplementationMap.Values;
}
