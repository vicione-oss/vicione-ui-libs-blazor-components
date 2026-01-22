#pragma warning disable IDE0005 // Using directive is unnecessary.
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NTypewriter.CodeModel;
#pragma warning restore IDE0005 // Using directive is unnecessary.

namespace ClassTemplate;

/// <summary>
/// https://github.com/NeVeSpl/NTypewriter/blob/master/Documentation/CustomFunctions.md
/// </summary>
public static class CustomFunctions
{
    /// <summary>
    /// Generates import statements for the given class based on properties that have specific attributes.
    /// </summary>
    public static string GenerateImports(IClass @class)
    {
        var imports = new List<string>();

        foreach (var property in @class.Properties)
        {
            if (property.Type.Attributes.Any(attribute => attribute.Name is "GenerateTypeScriptClass" or "GenerateTypeScriptEnum"))
            {
                var tsType = property.Type.Name;

                imports.Add($"import {{ type {tsType} }} from '/_content/ViciOne.Ui.Blazor.Components/js/{tsType.ToDashCase()}.js';");
            }
        }

        if (imports.Count > 0)
            return string.Join(Environment.NewLine, imports) + Environment.NewLine;
        else
            return string.Empty;
    }

    private static string ToDashCase(this string s)
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
}
