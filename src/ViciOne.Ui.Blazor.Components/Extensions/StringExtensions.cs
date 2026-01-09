using System.Text;

namespace ViciOne.Ui.Blazor.Components.Extensions;

/// <summary>
/// Extension methods for the string type.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string from UpperCamelCase/PascalCase to kebab-case.
    /// Inserts a hyphen before each uppercase letter (except the first),
    /// then lowercases the letters. Other characters are preserved.
    /// </summary>
    /// <param name="s">The input string, for example SaveAsDialog.</param>
    /// <returns>
    /// The kebab-case representation, for example save-as-dialog.
    /// Returns the input unchanged when it is null or empty.
    /// </returns>
    public static string ToDashCase(this string s)
    {
        // https://stackoverflow.com/a/57517576/3936440

        if (string.IsNullOrEmpty(s))
            return s;

        var sb = new StringBuilder();

        foreach (var ch in s)
        {
            if (char.IsUpper(ch))
            {
                if (sb.Length > 0)
                    sb.Append('-');

                sb.Append(char.ToLowerInvariant(ch));
            }
            else
            {
                sb.Append(ch);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Joins the given values with a single space as the separator.
    /// </summary>
    /// <param name="values">The sequence of strings to join.</param>
    /// <returns>
    /// A space-separated string in the form {value1} {value2} {value3}.
    /// </returns>
    public static string ToSpaceSeparated(this IEnumerable<string> values)
        => string.Join(" ", values);
}
