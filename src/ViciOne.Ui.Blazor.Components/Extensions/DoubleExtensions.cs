using System.Collections.Concurrent;
using System.Globalization;

namespace ViciOne.Ui.Blazor.Components.Extensions;

/// <summary>
/// Extension methods to localize values of type <see cref="double"/>
/// </summary>
public static class DoubleExtensions
{
    // Concurrent because renders of different circuits run on different threads: a plain dictionary written
    // from two of them at once corrupts and then throws on every later read for the lifetime of the process.
    private static readonly ConcurrentDictionary<int, string> s_attributeValueFormats = [];

    /// <summary>
    /// Converts the given <paramref name="value"/> to a custom invariant notation for use in HTML attributes.
    /// The notation only includes decimal digits when <paramref name="value"/> contains decimal digits.
    /// When <paramref name="precision"/> is smaller than the number of decimal digits then rounding is applied.
    /// </summary>
    public static string ToAttributeValue(this double value, int precision)
    {
        var culture = CultureInfo.InvariantCulture;

        if (precision < 0)
            precision = 0;

        // https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings
        var format = s_attributeValueFormats.GetOrAdd(precision,
            static digitCount => "{0:0." + new string('#', digitCount) + "}");

        return string.Format(culture, format, value);
    }

    /// <summary>
    /// Compares the given values against each other. If they are within one <paramref name="epsilon"/> of each other, returns true.
    /// Also returns true if both values are null.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note: This function is <b>not transitive</b>.
    /// If <c>a.NearlyEquals(b)</c> is <see langword="true"/> and <c>b.NearlyEquals(c)</c> is <see langword="true"/>,
    /// do not rely on <c>a.NearlyEquals(c)</c> being <see langword="true"/>.
    /// </para>
    /// </remarks>
    internal static bool NearlyEquals(this double? a, double? b, double epsilon = 0.001)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Value.NearlyEquals(b, epsilon);
    }

    /// <summary>
    /// Compares the given values against each other. If they are within one <paramref name="epsilon"/> of each other, returns true.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note: This function is <b>not transitive</b>.
    /// If <c>a.NearlyEquals(b)</c> is <see langword="true"/> and <c>b.NearlyEquals(c)</c> is <see langword="true"/>,
    /// do not rely on <c>a.NearlyEquals(c)</c> being <see langword="true"/>.
    /// </para>
    /// </remarks>
    internal static bool NearlyEquals(this double a, double? b, double epsilon = 0.001)
    {
        if (b is null)
            return false;

        return Math.Abs(a - b.Value) < epsilon;
    }
}
