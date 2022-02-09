using Data.Models.Enums;
using Runners.Abstractions;

namespace Api.Services.Abstractions;

public interface ITestableAppFactory
{
    ITestableApp CreateFromType(ProblemType problemType);
    ITestableApp CreateFromDescription(string language, string solutionType);
}
