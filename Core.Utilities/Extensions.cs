using System.Diagnostics;
using Core.Types;

namespace Core.Utilities;

public static class EnumerableExtender
{
    public static string StringJoin<T>(this IEnumerable<T> enumerable, string separator = "")
        => string.Join(separator, enumerable);
}

public static class StreamExtender
{
    public static async Task<byte[]> CollectAsByteArrayAsync(this Stream stream)
    {
        await using var stream0 = stream;
        await using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }
}

public static class ProcessExtensions
{
    public static async Task<Result<bool, Exception>> WaitForSuccessfulExitAsync(this Process process, uint waitTimeMs = 128)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(waitTimeMs));
        var result = await Ops.RunCatchingAsync(() => process.WaitForExitAsync(cts.Token));
        if (result is None<bool, Exception> errorResult)
        {
            var error = await process.CollectErrorOutput();
            return new None<bool, Exception>(new Exception(error, errorResult.Error));
        }

        if (process.ExitCode != 0)
        {
            var error = await process.CollectErrorOutput();
            return new None<bool, Exception>(new Exception(error));
        }

        return new Some<bool, Exception>(true);
    }

    public static async Task<string> CollectErrorOutput(this Process process)
    {
        var error = await process.StandardError.ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(error))
        {
            error = await process.StandardOutput.ReadToEndAsync();
        }

        return error;
    }
}
