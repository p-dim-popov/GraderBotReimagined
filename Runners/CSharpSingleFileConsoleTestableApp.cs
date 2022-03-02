using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using Core.Types;
using Core.Utilities;
using Helpers;

namespace Runners;

public class CSharpSingleFileConsoleTestableApp: BaseConsoleTestableApp
{
    private const string TargetFramework = "net6.0";
    private const string ProjectName = "ConsoleApp";
    private const bool SelfContained = true;
    private const string RuntimeIdentifier = "linux-x64";
    private const string CsProj = $@"
<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>{TargetFramework}</TargetFramework>
        {(SelfContained ? $"<RuntimeIdentifier>{RuntimeIdentifier}</RuntimeIdentifier>" : "")}
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
    </PropertyGroup>
</Project>
";

    private Task<Result<string, Exception>>? _prepareTask;

    public CSharpSingleFileConsoleTestableApp(IProcessStarter processStarter) : base(processStarter)
    { }

    protected override async Task<Result<string, Exception>> RunAsync(DirectoryInfo workingDirectory, byte[] solution, JsonValue[] inputLines)
    {
        _prepareTask ??= PrepareAsync(workingDirectory, solution);
        var binaryPathResult = await _prepareTask;

        if (binaryPathResult is None<string, Exception> errorResult) return new None<string, Exception>(errorResult.Error);
        var binaryPath = (binaryPathResult as Some<string, Exception>)!.Result;

        var process = (_processStarter.Start(
            "firejail",
            new []{"--quiet", "--net=none", binaryPath}
        ) as Some<Process, object>)!.Result;

        foreach (var inputLine in inputLines)
        {
            await process.StandardInput.WriteLineAsync(inputLine.ToString());
        }

        if (await process.WaitForSuccessfulExitAsync(512) is None<bool, Exception> result)
            return new None<string, Exception>(result.Error);

        var output = $"{await process.StandardOutput.ReadToEndAsync()}".TrimEnd();
        return new Some<string, Exception>(output);
    }

    protected override DirectoryInfo GetWorkingDirectoryPerInput(DirectoryInfo baseWorkDir, int index) => baseWorkDir;

    private async Task<Result<string, Exception>> PrepareAsync(DirectoryInfo directoryInfo, byte[] solution)
    {
        await CreateProjectAsync(directoryInfo, solution);
        return await CompileAsync(directoryInfo);
    }

    private async Task CreateProjectAsync(DirectoryInfo directory, byte[] solution)
    {
        var solutionCode = Encoding.UTF8.GetString(solution, 0, solution.Length).Trim();
        await Task.WhenAll(
            FileOps.WriteFileAsync(Path.Join(directory.FullName, "Program.cs"), solutionCode),
            FileOps.WriteFileAsync(Path.Join(directory.FullName, $"{ProjectName}.csproj"), CsProj)
        );
    }

    private async Task<Result<string, Exception>> CompileAsync(DirectoryInfo workingDirectory)
    {
        var process = (_processStarter.Start(
            "dotnet",
            new []
            {
                "build",
                "--nologo",
                "--verbosity quiet",
                SelfContained ? "--sc" : "",
                workingDirectory.FullName
            }
        ) as Some<Process, object>)!.Result;

        if (await process.WaitForSuccessfulExitAsync(5_000) is None<bool, Exception> result)
            return new None<string, Exception>(result.Error);

        var binaryPath = Path.Join(
            workingDirectory.FullName, "bin", "Debug", TargetFramework, SelfContained ? RuntimeIdentifier : "", ProjectName
        );
        return new Some<string, Exception>(binaryPath);
    }
}
