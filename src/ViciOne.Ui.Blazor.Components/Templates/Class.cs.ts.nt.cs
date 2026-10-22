#pragma warning disable IDE0005 // Using directive is unnecessary.
using System;
using System.Collections.Generic;
using System.IO;
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

        // Must match the condition under which the template writes "BlazorNullable<...>"
        if (@class.Properties.Any(property => property.IsPublic && property.Type.IsNullable))
            imports.Add($"import {{ type BlazorNullable }} from '{GetBlazorNullableModulePath(@class)}';");

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

    /// <summary>
    /// Returns the path of Scripts/BlazorNullable.ts relative to the TypeScript file generated for the given class,
    /// for example "../../Scripts/BlazorNullable.ts" for a class in Draggable/Models.
    /// </summary>
    private static string GetBlazorNullableModulePath(IClass @class)
    {
        var classDirectory = Path.GetDirectoryName(@class.Locations.First().Path)!;

        // The project root is the closest folder upwards that holds the project file
        var projectDirectory = classDirectory;
        while (!Directory.EnumerateFiles(projectDirectory, "*.csproj").Any())
        {
            projectDirectory = Path.GetDirectoryName(projectDirectory)
                ?? throw new InvalidOperationException($"No project file found above '{classDirectory}'.");
        }

        var modulePath = Path.Combine(projectDirectory, "Scripts", "BlazorNullable.ts");
        var relativePath = GetRelativePath(classDirectory, modulePath).Replace('\\', '/');

        return relativePath.StartsWith(".", StringComparison.Ordinal) ? relativePath : "./" + relativePath;
    }

    // Path.GetRelativePath does not exist in .NET Standard 2.0, which custom functions may be compiled against
    private static string GetRelativePath(string fromDirectory, string toFile)
    {
        var trimmedFromDirectory = fromDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fromUri = new Uri(trimmedFromDirectory + Path.DirectorySeparatorChar);
        var toUri = new Uri(toFile);

        return Uri.UnescapeDataString(fromUri.MakeRelativeUri(toUri).ToString());
    }
}
