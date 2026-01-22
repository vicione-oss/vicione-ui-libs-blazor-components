#pragma warning disable IDE0005 // Using directive is unnecessary.
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NTypewriter.CodeModel;
#pragma warning restore IDE0005 // Using directive is unnecessary.

namespace NTypeWriter.CodeModel.Customization;

/// <summary>
/// Provides custom functions for NTypeWriter code generation.
/// </summary>
/// <remarks>
/// https://github.com/NeVeSpl/NTypewriter/blob/master/Documentation/CustomFunctions.md
/// </remarks>
internal static class CustomFunctions
{
    /// <summary>
    /// Generates import statements for property types
    /// </summary>
    public static string GenerateImports(IStruct @struct)
    {
        const string ProjectNamespace = "ViciOne.Ui.Blazor.Components";

        var imports = new List<string>();

        foreach (var property in @struct.Properties)
        {
            if (property.Type.Attributes.Any(a => a.Name is "GenerateTypeScriptEnum"))
            {
                var typeNamespace = property.Type.Namespace;

                if (property.Type.Namespace.StartsWith(ProjectNamespace, StringComparison.Ordinal))
                    typeNamespace = property.Type.Namespace.Substring(ProjectNamespace.Length + 1);

                var relativeStaticWebAssetPath = string.Join("/", typeNamespace.Split('.').Select(n => n.ToDashCase()));

                var tsType = property.Type.Name;
                var tsFilePath = $"/_content/{ProjectNamespace}/{relativeStaticWebAssetPath}";

                imports.Add($"import {{ type {tsType} }} from '{tsFilePath}/{tsType.ToDashCase()}.js';");
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
