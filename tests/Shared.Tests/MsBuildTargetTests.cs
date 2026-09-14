using System.Diagnostics;

namespace Shared.Tests;

public sealed class MsBuildTargetTests
{
    [Fact]
    public async Task Should_rewrite_imports_during_shared_project_build()
    {
        // Arrange
        var repositoryRoot = GetRepositoryRoot();
        var sharedDirectory = Path.Combine(repositoryRoot, "samples", "Shared");
        var sharedProjectPath = Path.Combine(sharedDirectory, "Shared.csproj");
        var generatedJavaScriptPath = Path.Combine(sharedDirectory, "dist", "js", "computed-background-color-page.js");

        // Act
        var result = await RunDotNetAsync(sharedDirectory, "build", sharedProjectPath, "--configuration", "Release", "--no-restore",
            "--no-incremental");

        // Assert
        result.ExitCode.Should().Be(0, "Shared build should succeed. Output: {0}", result.Output);

        File.Exists(generatedJavaScriptPath).Should().BeTrue("generated asset should exist: {0}", generatedJavaScriptPath);

        var generatedJavaScript = await File.ReadAllTextAsync(generatedJavaScriptPath, TestContext.Current.CancellationToken);

        generatedJavaScript.Should().Contain("from '../../ViciOne.Ui.Blazor.Components/js/computed-background-color.js';");
        generatedJavaScript.Should().Contain("import '../../ViciOne.Ui.Blazor.Components/js/html-element-mixins.js';");
    }

    private static string GetRepositoryRoot()
    {
        var assemblyDirectory = Path.GetDirectoryName(typeof(MsBuildTargetTests).Assembly.Location);

        assemblyDirectory.Should().NotBeNull();

        return Path.GetFullPath(Path.Combine(assemblyDirectory, "..", "..", "..", "..", ".."));
    }

    private static async Task<ProcessResult> RunDotNetAsync(string workingDirectory, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start().Should().BeTrue("dotnet should start");

        var standardOutputTask = process.StandardOutput.ReadToEndAsync();
        var standardErrorTask = process.StandardError.ReadToEndAsync();

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        timeout.CancelAfter(TimeSpan.FromMinutes(5));

        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);

            throw;
        }

        var standardOutput = await standardOutputTask;
        var standardError = await standardErrorTask;

        return new ProcessResult(process.ExitCode, standardOutput + Environment.NewLine + standardError);
    }

    private sealed record ProcessResult(int ExitCode, string Output);
}
