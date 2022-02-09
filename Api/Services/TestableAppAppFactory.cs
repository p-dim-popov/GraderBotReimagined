using Api.Helpers;
using Api.Services.Abstractions;
using Data.Models.Enums;
using Helpers;
using Runners;
using Runners.Abstractions;

namespace Api.Services;

public class TestableAppFactory: ITestableAppFactory
{
    private readonly IProcessStarter _processStarter;

    public TestableAppFactory(IProcessStarter processStarter)
    {
        _processStarter = processStarter;
    }

    public ITestableApp CreateFromType(ProblemType problemType)
    {
        return problemType switch
        {
            ProblemType.JavaScriptSingleFileConsoleApp => new JavaScriptSingleFileConsoleTestableApp(_processStarter),
            _ => new NotSupportedTestableApp(),
        };
    }

    public ITestableApp CreateFromDescription(string language, string solutionType)
    {
        return ProblemTypeResolver.Resolve(language, solutionType) switch
        {
            {} type => CreateFromType(type),
            _ => new NotSupportedTestableApp(),
        };
    }
}
