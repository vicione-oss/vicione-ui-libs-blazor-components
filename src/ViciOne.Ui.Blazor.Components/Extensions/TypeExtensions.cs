using System.ComponentModel;

namespace ViciOne.Ui.Blazor.Components.Extensions;
/// <summary>
/// Extension methods for <see cref="Type"/>.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Returns the non-nullable counterpart of the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">A nullable or non-nullable type.</param>
    /// <returns>
    /// The underlying non-nullable type if <paramref name="type"/> is a closed <see cref="Nullable{T}"/>;
    /// otherwise <paramref name="type"/> itself.
    /// </returns>
    public static Type MakeNonNullableType(this Type type)
    {
        if (type.IsNullableValueType())
            return new NullableConverter(type).UnderlyingType;

        return type;
    }

    /// <summary>
    /// Determines whether the given <paramref name="type"/> is a closed <see cref="Nullable{T}"/>.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="type"/> is a nullable value type (i.e., <c>Nullable&lt;T&gt;</c>);
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool IsNullableValueType(this Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

    /// <summary>
    /// Returns a nullable version of the given value <paramref name="type"/>, if applicable.
    /// </summary>
    /// <param name="type">A nullable or non-nullable type.</param>
    /// <returns>
    /// If <paramref name="type"/> is a non-nullable value type, returns <c>Nullable&lt;<paramref name="type"/>&gt;</c>;
    /// otherwise returns <paramref name="type"/> unchanged.
    /// </returns>
    public static Type MakeNullableType(this Type type)
    {
        if (type.IsValueType && !type.IsNullableValueType())
            return typeof(Nullable<>).MakeGenericType(type);

        return type;
    }
}
