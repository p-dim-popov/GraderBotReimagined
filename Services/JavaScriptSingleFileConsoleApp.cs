using System.Diagnostics;
using System.Text.Json;
using Contracts;
using Utilities;

namespace Services;

public class JavaScriptSingleFileConsoleApp: IConsoleApp
{
    private readonly IProgramRunner _programRunner;

    public JavaScriptSingleFileConsoleApp(IProgramRunner programRunner)
    {
        _programRunner = programRunner;
    }

    public async Task<Result<string, Exception>> RunAsync(DirectoryInfo directory, string input)
    {
        var file = directory.GetFiles().FirstOrDefault();
        if (file is null) return new ErrorResult<string, Exception> { None = new FileNotFoundException("Could not find solution file")};

        var jsonArgs = GetArgsAsJsonArray(input);
        var mainJs = await CreateMainJsAsync(directory, file, jsonArgs);

        var process = _programRunner.Run("node", mainJs);

        if (await WaitForSuccessfulExitAsync(process) is ErrorResult<string, Exception> result) return result;

        var output = $"{await process.StandardOutput.ReadToEndAsync()}".Trim();
        File.Delete(mainJs);
        return new SuccessResult<string, Exception> { Some = output };
    }

    private static async Task<Result<string, Exception>> WaitForSuccessfulExitAsync(Process process)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(128));
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (Exception exception)
        {
            var error = await process.StandardError.ReadToEndAsync();
            return new ErrorResult<string, Exception> {None = new Exception(error, exception)};
        }

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            return new ErrorResult<string, Exception> {None = new Exception(error)};
        }

        return new SuccessResult<string, Exception>();
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