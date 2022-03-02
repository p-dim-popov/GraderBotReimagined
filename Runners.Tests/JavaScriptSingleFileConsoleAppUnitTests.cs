using System.Threading.Tasks;
using Helpers;
using NUnit.Framework;

namespace Runners.Tests;

public class JavaScriptSingleFileConsoleAppUnitTests: BaseConsoleAppUnitTests
{
    public JavaScriptSingleFileConsoleAppUnitTests() : base(() => new JavaScriptSingleFileConsoleTestableApp(new ProcessStarter()))
    { }

    [TestCase("unknown", "Error: ")]
    [TestCase(";", "error: The module's source code could not be parsed")]
    public override Task SolutionWithErrors_ShouldResultToErrorResult(string function, string expected) => base.SolutionWithErrors_ShouldResultToErrorResult(function, expected);

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
    [TestCase("() => console.log('comma at end of function declaration');", "[[]]", "comma at end of function declaration")]
    [TestCase("() => console.log('newlines and comma at end of function declaration')\n\n;", "[[]]", "newlines and comma at end of function declaration")]
    [TestCase("() => console.log('newlines and comma and more newlines at end of function declaration')\n\n;\n", "[[]]", "newlines and comma and more newlines at end of function declaration")]
    public override Task SolutionWithoutErrors_ShouldWorkAsExpected(string function, string input, string expected) => base.SolutionWithoutErrors_ShouldWorkAsExpected(function, input, expected);

    protected override string LogFirstLineFromInputSolution => "li => li.forEach(i => console.log(i))";
}
