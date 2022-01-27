namespace Utilities;

public static class EnumerableExtender
{
    public static string StringJoin<T>(this IEnumerable<T> enumerable, string separator = "")
        => string.Join(separator, enumerable);
}
