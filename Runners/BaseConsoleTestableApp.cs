using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Core.Types;
using Helpers;
using Runners.Abstractions;

namespace Runners;

public abstract class BaseConsoleTestableApp: ITestableApp
{
    protected readonly IProcessStarter _processStarter;

    protected BaseConsoleTestableApp(IProcessStarter processStarter)
    {
        _processStarter = processStarter;
    }

    public async Task<Result<Result<string, Exception>[], Exception>> TestAsync(DirectoryInfo workDir, byte[] solution, byte[] inputBytes)
    {
        var inputs = GetInputs(inputBytes);
        if (inputs is null) return new None<Result<string, Exception>[], Exception>(new Exception("Input not valid"));

        var results = await Task.WhenAll(inputs.Select((inputLines, i) => RunAsync(GetWorkingDirectoryPerInput(workDir, i), solution, inputLines)));

        return new Some<Result<string, Exception>[], Exception>(results);
    }

    protected virtual DirectoryInfo GetWorkingDirectoryPerInput(DirectoryInfo baseWorkDir, int index) => baseWorkDir.CreateSubdirectory(index.ToString());

    private static JsonValue[][]? GetInputs(byte[] inputBytes)
    {
        var jsonInput = Encoding.UTF8.GetString(inputBytes, 0, inputBytes.Length);
        var inputs = JsonSerializer.Deserialize<JsonValue[][]>(jsonInput);
        return inputs;
    }

    protected abstract Task<Result<string, Exception>> RunAsync(DirectoryInfo workingDirectory, byte[] solution, JsonValue[] inputLines);
}
