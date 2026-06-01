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

        var generateTypeScriptImportAttributes = @class.Attributes.Where(attribute => attribute.Name is "GenerateTypeScriptImport");
        foreach (var attribute in generateTypeScriptImportAttributes)
        {
            var typeArgument = attribute.Arguments.FirstOrDefault(a => a.Name == "Type");
            var modulePathArgument = attribute.Arguments.FirstOrDefault(a => a.Name == "ModulePath");

            if (typeArgument is not null && modulePathArgument is not null)
                imports.Add($"import {{ type {typeArgument.Value} }} from '{modulePathArgument.Value}';");
        }

        const string LineBreak = "\n";

        if (imports.Count > 0)
            return string.Join(LineBreak, imports) + LineBreak;
        else
            return string.Empty;
    }
}
