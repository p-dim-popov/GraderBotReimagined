using Core.Types;

namespace Runners.Abstractions;

public interface IApp
{
    Task<Result<string, Exception>> RunAsync(DirectoryInfo directory, string input);
}
