using Data.Models.Enums;

namespace Api.Helpers;

public static class ProblemTypeResolver
{
    public static ProblemType? Resolve(string? programmingLanguage, string? type)
    {
        return programmingLanguage switch
        {
            "javascript" => type switch
            {
                "single-file-console-app" => ProblemType.JavaScriptSingleFileConsoleApp,
                _ => null,
            },

            _ => null,
        };
    }
}
