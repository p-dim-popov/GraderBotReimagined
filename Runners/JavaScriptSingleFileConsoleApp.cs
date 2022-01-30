using System.Diagnostics;
using System.Text.Json;
using Core.Types;
using Core.Utilities;
using Helpers;
using Runners.Abstractions;

namespace Runners;

public class JavaScriptSingleFileConsoleApp : IApp
{
    private readonly IProcessStarter _processStarter;

    public JavaScriptSingleFileConsoleApp(IProcessStarter processStarter)
    {
        _processStarter = processStarter;
    }

    public async Task<Result<string, Exception>> RunAsync(DirectoryInfo directory, string input)
    {
        var file = directory.GetFiles().FirstOrDefault();
        if (file is null)
            return new ErrorResult<string, Exception>(new FileNotFoundException("Could not find solution file"));

        var jsonArgs = GetArgsAsJsonArray(input);
        var mainJs = await CreateMainJsAsync(directory, file, jsonArgs);

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

    private static string GetArgsAsJsonArray(string input)
    {
        var args = input.Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);
        var jsonArgs = JsonSerializer.Serialize(args);
        return jsonArgs;
    }

    private static async Task<string> CreateMainJsAsync(DirectoryInfo directory, FileInfo file, string jsonArgs)
    {
        var function = $"{await File.ReadAllTextAsync(file.FullName)}".Trim();
        var mainFnBody = $"({function})(JSON.parse(`{jsonArgs}`))";
        var mainJsFilename = Path.Join(directory.FullName, "__main__.js");
        await FileOps.WriteFileAsync(mainJsFilename, mainFnBody);

        return mainJsFilename;
    }
}