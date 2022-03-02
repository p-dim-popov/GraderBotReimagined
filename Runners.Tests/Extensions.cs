using Newtonsoft.Json;

namespace Runners.Tests;

public static class ObjectExtensions
{
    public static string ToReadableJson<T>(this T obj) => JsonConvert.SerializeObject(obj, Formatting.Indented);
}
