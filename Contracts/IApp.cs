using Utilities;

namespace Contracts;

public interface IApp
{
    Task<Result<string, Exception>> RunAsync(DirectoryInfo directory, string input);
}
