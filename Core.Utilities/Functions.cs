namespace Core.Utilities;

public static class FileOps
{
    public static async Task WriteFileAsync(string filename, string body)
    {
        await using var writer = File.CreateText(filename);
        await writer.WriteLineAsync(body);
    }
}