using System.Threading.Tasks;
using Helpers;
using NUnit.Framework;

namespace Runners.Tests;

public class CSharpSingleFileConsoleAppUnitTests: BaseConsoleAppUnitTests
{
    public CSharpSingleFileConsoleAppUnitTests() : base(() =>
        new CSharpSingleFileConsoleTestableApp(new ProcessStarter()))
    {
    }

    [TestCase("Console.WriteLine(123)", "error CS1002: ; expected")]
    [TestCase("console.writeLine(123);", "error CS0103: The name 'console' does not exist in the current context")]
    [TestCase("Console.WriteLine(int.Parse(Console.ReadLine()));", "Unhandled exception. System.FormatException: Input string was not in a correct format.")]
    public override Task SolutionWithErrors_ShouldResultToErrorResult(string function, string expected) => base.SolutionWithErrors_ShouldResultToErrorResult(function, expected);

    [TestCase(
        "Enumerable.Range(0, 3).Select(_ => Console.ReadLine()).ToList().ForEach(Console.WriteLine);",
        @"[[""first string"",""some number next"",""123""]]",
        "first string\nsome number next\n123"
    )]
    [TestCase(
        "Enumerable.Range(0, 3).Select(_ => Console.ReadLine()).Select(int.Parse).ToList().ForEach(Console.WriteLine);",
        @"[[""2"",""3"",""0"",""1""]]",
        "2\n3\n0"
    )]
    public override Task SolutionWithoutErrors_ShouldWorkAsExpected(string function, string input, string expected) => base.SolutionWithoutErrors_ShouldWorkAsExpected(function, input, expected);

    protected override string LogFirstLineFromInputSolution => "Console.WriteLine(Console.ReadLine());";
}
