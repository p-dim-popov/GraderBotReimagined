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
