using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Types;
using NUnit.Framework;
using Runners.Abstractions;

namespace Runners.Tests;

public abstract class BaseConsoleAppUnitTests: BaseUnitTests
{
    protected BaseConsoleAppUnitTests(Func<ITestableApp> createTestableApp) : base(createTestableApp)
    { }

    [Test]
    public virtual async Task SolutionWithErrors_ShouldResultToErrorResult(string function, string expected)
    {
        var runResult = await TestAsync(function, @"[[""123""], [""hello""], [""darkness...""]]");

        var errorResults = runResult.OfType<None<string, Exception>>().ToList();
        Assert.Greater(errorResults.Count, 1, runResult.ToReadableJson());
        errorResults.ForEach(errorResult => StringAssert.Contains(expected, errorResult.Error.Message, runResult.ToReadableJson()));
    }

    [Test]
    public virtual async Task SolutionWithoutErrors_ShouldWorkAsExpected(string function, string input, string expected)
    {
        var runResult = await TestAsync(function, input);

        var successResult = runResult.First() as Some<string, Exception>;
        Assert.NotNull(successResult, runResult.ToReadableJson());
        Assert.AreEqual(expected, successResult?.Result, runResult.ToReadableJson());
    }

    private async Task<Result<string, Exception>[]> TestAsync(string function, string input)
    {
        var runResult = await _testableApp.TestAsync(
            _solutionDir,
            Encoding.UTF8.GetBytes(function),
            Encoding.UTF8.GetBytes(input)
        ) as Some<Result<string, Exception>[], Exception>;

        Assert.IsNotNull(runResult);
        return runResult!.Result;
    }

    [Test]
    [TestCase(@"[[""trim me  ""]]", @"trim me")]
    [TestCase(@"[["" do not trim me ""]]", @" do not trim me")]
    public async Task Result_ShouldBeTrimmed(string input, string expected)
    {
        var runResult = await TestAsync(LogFirstLineFromInputSolution, input);

        var successResult = runResult.First() as Some<string, Exception>;
        Assert.IsNotNull(successResult, runResult.ToReadableJson());
        Assert.AreEqual(expected, successResult?.Result, runResult.ToReadableJson());
    }

    protected abstract string LogFirstLineFromInputSolution { get; }
}
