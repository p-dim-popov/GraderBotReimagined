using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Types;
using Core.Utilities;
using Helpers;
using NUnit.Framework;

namespace Runners.Tests;

public class JavaScriptSingleFileConsoleAppUnitTests
{
    private readonly JavaScriptSingleFileConsoleTestableApp _testableApp = new(new ProcessStarter());
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
        var runResult = await _testableApp.TestAsync(
            _solutionDir,
            Encoding.UTF8.GetBytes(function),
            Encoding.UTF8.GetBytes(@"[[""first string"",""some number next"",""123""]]"
            )) as SuccessResult<Result<string, Exception>[], Exception>;
        Assert.IsNotNull(runResult);

        var errorResult = runResult!.Some.FirstOrDefault() as ErrorResult<string, Exception>;

        Assert.IsNotNull(errorResult);
        StringAssert.Contains(expected, errorResult?.None.Message);
    }

    [Test]
    [TestCase(
        "console.log",
        @"[[""first string"",""some number next"",""123""]]",
        @"[ ""first string"", ""some number next"", ""123"" ]"
    )]
    [TestCase(
        "(input) => input.map(Number).filter(Boolean).filter(x => x % 2).forEach(console.log)",
        @"[[""2"",""3"",""X"",""1""]]",
        "3 0 [ 3, 1 ]\n1 1 [ 3, 1 ]"
    )]
    public async Task CallingFunctionWithoutErrors_ShouldWorkAsExpected(string function, string input, string expected)
    {
        var runResult = await _testableApp.TestAsync(
            _solutionDir,
            Encoding.UTF8.GetBytes(function),
            Encoding.UTF8.GetBytes(input)
        ) as SuccessResult<Result<string, Exception>[], Exception>;
        Assert.IsNotNull(runResult);

        var successResult = runResult?.Some.FirstOrDefault() as SuccessResult<string, Exception>;

        Assert.IsNotNull(successResult);
        Assert.AreEqual(expected, successResult?.Some);
    }

    [Test]
    [TestCase(@"[[""trim me""]]", @"[ ""trim me"" ]")]
    [TestCase(@"[["" do not trim me ""]]", @"[ "" do not trim me "" ]")]
    public async Task Result_ShouldBeTrimmed(string input, string expected)
    {
        var runResult = await _testableApp.TestAsync(
            _solutionDir,
            Encoding.UTF8.GetBytes("console.log"),
            Encoding.UTF8.GetBytes(input)
        ) as SuccessResult<Result<string, Exception>[], Exception>;
        Assert.IsNotNull(runResult);

        var successResult = runResult?.Some.FirstOrDefault() as SuccessResult<string, Exception>;
        Assert.IsNotNull(successResult);
        Assert.AreEqual(expected, successResult?.Some);
    }
}
