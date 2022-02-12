using System.Text.Json.Serialization;
using Data.Models.Enums;

namespace Api.Models.Problem;

public record ProblemTypeDescription(
    string Language,
    string SolutionType,
    [property: JsonIgnore]
    ProblemType Type,
    string DisplayName,
    string Description
)
{
    public static ProblemTypeDescription[] List { get; } = {
        new(
            "javascript",
            "single-file-console-app",
            ProblemType.JavaScriptSingleFileConsoleApp,
            "JavaScript Single File Console App",
            "Console app written in one file"
        )
    };
};
