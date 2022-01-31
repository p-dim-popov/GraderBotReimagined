using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace Data.DbContexts;

public class LocalDbContext: AppDbContext
{
    private static string GetThisFilePath([CallerFilePath] string path = "") => path;

    private static Exception LostException() => new("I do not know where I am!?!");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var projectDir = Directory.GetParent(GetThisFilePath()) switch
        {
            { } dbContexts => dbContexts.Parent switch
            {
                { } project => project,
                _ => throw LostException(),
            },
            _ => throw LostException(),
        };

        var path = Path.Join(projectDir.FullName, "app.db");
        optionsBuilder.UseSqlite($"data source={path}");
    }
}
