using System.Globalization;

namespace ViciOne.Ui.Blazor.Components.Extensions;

/// <summary>
/// Extension methods to localize values of type <see cref="double"/>
/// </summary>
public static class DoubleExtensions
{
    private static readonly Dictionary<int, string> s_attributeValueFormats = [];

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

        if (!s_attributeValueFormats.TryGetValue(precision, out var format))
        {
            // https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings
            var decimalDigitFormat = new string('#', precision);
            format = "{0:0." + decimalDigitFormat + "}";

            s_attributeValueFormats[precision] = format;
        }

        return string.Format(culture, format, value);
    }
}
