using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Core.Types;
using Core.Utilities;
using Helpers;

namespace Runners;

public class JavaScriptSingleFileConsoleTestableApp : BaseConsoleTestableApp
{
    private static readonly Regex MatchSemicolonsFromEnd = new Regex("(;+\n*)+$", RegexOptions.Multiline);

    public JavaScriptSingleFileConsoleTestableApp(IProcessStarter processStarter) : base(processStarter)
    { }

    protected override async Task<Result<string, Exception>> RunAsync(DirectoryInfo workingDirectory, byte[] solution, JsonValue[] inputLines)
    {
        var mainJs = await CreateMainJsAsync(workingDirectory, solution, inputLines);

        var process = (_processStarter.Start(
            "deno",
            new []{$"run --quiet {mainJs}"},
            new Dictionary<string, string>{ {"NO_COLOR", bool.TrueString}}
        ) as SuccessResult<Process, object>)!.Some;

        if (await process.WaitForSuccessfulExitAsync() is ErrorResult<bool, Exception> result)
            return new ErrorResult<string, Exception>(result.None);

        var output = $"{await process.StandardOutput.ReadToEndAsync()}".TrimEnd();
        File.Delete(mainJs);
        return new SuccessResult<string, Exception>(output);
    }

    private static async Task<string> CreateMainJsAsync(DirectoryInfo directory, byte[] solution, JsonValue[] args)
    {
        var rawSolution = Encoding.UTF8.GetString(solution, 0, solution.Length);
        var function = MatchSemicolonsFromEnd.Replace(rawSolution, "").Trim();
        var mainFnBody = $"({function})(JSON.parse(`{JsonSerializer.Serialize(args)}`))";
        var mainJsFilename = Path.Join(directory.FullName, "__main__.js");
        await FileOps.WriteFileAsync(mainJsFilename, mainFnBody);

        return mainJsFilename;
    }
}
