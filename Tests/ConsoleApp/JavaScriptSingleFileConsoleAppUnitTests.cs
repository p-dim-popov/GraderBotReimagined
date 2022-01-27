using System;
using System.IO;
using System.Threading.Tasks;
using Contracts;
using NUnit.Framework;
using Services;
using Utilities;

namespace Tests.ConsoleApp;

public class JavaScriptSingleFileConsoleAppUnitTests
{
    private readonly IConsoleApp _consoleApp = new JavaScriptSingleFileConsoleApp(new ProgramRunner());
    private DirectoryInfo _solutionDir = null!;
    private const string TempDir = "js-ca-test";

    [SetUp]
    public void Setup()
    {
        _solutionDir = Directory.CreateDirectory(Path.Join(Path.GetTempPath(), TempDir,
            $"{DateTime.Now:yyyy-MM-dd}--{Guid.NewGuid()}"));
    }

    [TearDown]
    public void Clean()
    {
        _solutionDir.Delete(true);
        _solutionDir = null!;
    }

    [Test]
    [TestCase("unknown", "Error: ")]
    public async Task CallingFunctionWithErrors_ShouldWorkAsExpected(string function, string expected)
    {
        await CreateMainFunction(function);
        var result =
            await _consoleApp.RunAsync(_solutionDir, "first string\r\nsome number next\r\n123") as
                ErrorResult<string, Exception>;

        Assert.IsNotNull(result);
        StringAssert.Contains(expected, result!.None.Message);
    }

    [Test]
    [TestCase(
        "console.log",
        "first string\r\nsome number next\r\n123",
        "[ 'first string', 'some number next', '123' ]"
    )]
    [TestCase(
        "(input) => input.map(Number).filter(Boolean).filter(x => x % 2).forEach(console.log)",
        "2\n3\nX\n1\n",
        "[ 3, 1 ]"
    )]
    public async Task CallingFunctionWithoutErrors_ShouldWorkAsExpected(string function, string input, string expected)
    {
        await CreateMainFunction(function);
        var result =
            await _consoleApp.RunAsync(_solutionDir, input) as
                SuccessResult<string, Exception>;

        Assert.IsNotNull(result);
        StringAssert.Contains(expected, result!.Some);
    }

    [Test]
    [TestCase("trim me", "[ 'trim me' ]")]
    [TestCase(" do not trim me ", "[ ' do not trim me ' ]")]
    public async Task Result_ShouldBeTrimmed(string input, string expected)
    {
        await CreateMainFunction("console.log");
        var result =
            await _consoleApp.RunAsync(_solutionDir, input) as
                SuccessResult<string, Exception>;

        Assert.IsNotNull(result);
        Assert.AreEqual(expected, result!.Some);
    }

    private Task CreateMainFunction(string function, string filename = "main.js") =>
        FileOps.WriteFileAsync(Path.Join(_solutionDir.FullName, filename), function);
}