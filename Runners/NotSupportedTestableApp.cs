using Core.Types;
using Runners.Abstractions;

namespace Runners;

public class NotSupportedTestableApp: ITestableApp
{
    public string? Language { get; init; }

    public string? Type { get; init; }

    public Task<Result<Result<string, Exception>[], Exception>> TestAsync(DirectoryInfo directory, byte[] inputBytes)
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
