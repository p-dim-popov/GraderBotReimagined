using Contracts;
using Utilities;

namespace Services;

public class NotSupportedApp: IApp
{
    public string? Language { get; init; }

    public string? Type { get; init; }
    
    public Task<Result<string, Exception>> RunAsync(DirectoryInfo directory, string input)
    {
        if (Language is not null)
        {
            throw new NotSupportedException($"Language {Language} is not supported!");
        }

        if (Type is not null)
        {
            throw new NotSupportedException($"Type {Type} is not supported!");
        }

        throw new NotSupportedException("Not supported!");
    }
}