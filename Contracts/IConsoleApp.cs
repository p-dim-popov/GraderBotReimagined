using Utilities;

namespace Contracts;

public interface IConsoleApp
{
    Task<Result<string, Exception>> RunAsync(DirectoryInfo directory, string input);
}
