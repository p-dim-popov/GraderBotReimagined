using Core.Types;

namespace Runners.Abstractions;

public interface ITestableApp
{
    Task<Result<Result<string, Exception>[], Exception>> TestAsync(
        DirectoryInfo workDir,
        byte[] solution,
        byte[] inputBytes
    );
}
