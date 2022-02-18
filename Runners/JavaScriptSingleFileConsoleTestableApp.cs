using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Core.Types;
using Core.Utilities;
using Helpers;
using Runners.Abstractions;

namespace Runners;

public class JavaScriptSingleFileConsoleTestableApp : ITestableApp
{
    private readonly IProcessStarter _processStarter;

    public JavaScriptSingleFileConsoleTestableApp(IProcessStarter processStarter)
    {
        _processStarter = processStarter;
    }

    public async Task<Result<Result<string, Exception>[], Exception>> TestAsync(
        DirectoryInfo workDir,
        byte[] solution,
        byte[] inputBytes
    )
    {
        var jsonInput = Encoding.UTF8.GetString(inputBytes, 0, inputBytes.Length);
        var inputs = JsonSerializer.Deserialize<JsonValue[][]>(jsonInput);
        if (inputs is null) return new ErrorResult<Result<string, Exception>[], Exception>(new Exception("Input not valid"));

        var results = await Task.WhenAll(inputs.Select((x, i) =>
        {
            var workingDirectory = workDir.CreateSubdirectory(i.ToString());
            return RunAsync(workingDirectory, solution, x);
        }));

        return new SuccessResult<Result<string, Exception>[], Exception>(results);
    }

    private async Task<Result<string, Exception>> RunAsync(DirectoryInfo workingDirectory, byte[] solution, JsonValue[] inputLines)
    {
        var mainJs = await CreateMainJsAsync(workingDirectory, solution, inputLines);

        var process = (_processStarter.Start("node", mainJs) as SuccessResult<Process, bool>)!.Some;

        if (await WaitForSuccessfulExitAsync(process) is ErrorResult<bool, Exception> result)
            return new ErrorResult<string, Exception>(result.None);

        var output = $"{await process.StandardOutput.ReadToEndAsync()}".Trim();
        File.Delete(mainJs);
        return new SuccessResult<string, Exception>(output);
    }

    private static async Task<Result<bool, Exception>> WaitForSuccessfulExitAsync(Process process)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(128));
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (Exception exception)
        {
            var error = await process.StandardError.ReadToEndAsync();
            return new ErrorResult<bool, Exception>(new Exception(error, exception));
        }

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            return new ErrorResult<bool, Exception>(new Exception(error));
        }

        return new SuccessResult<bool, Exception>(true);
    }

    private static async Task<string> CreateMainJsAsync(DirectoryInfo directory, byte[] solution, JsonValue[] args)
    {
        var function = $"{Encoding.UTF8.GetString(solution, 0, solution.Length)}".Trim();
        var mainFnBody = $"({function})(JSON.parse(`{JsonSerializer.Serialize(args)}`))";
        var mainJsFilename = Path.Join(directory.FullName, "__main__.js");
        await FileOps.WriteFileAsync(mainJsFilename, mainFnBody);

        return mainJsFilename;
    }
}
