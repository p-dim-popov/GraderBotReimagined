using Microsoft.EntityFrameworkCore;

namespace Data.DbContexts;

public class PgDbContext: AppDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var port = Environment.GetEnvironmentVariable("DB_PORT");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var database = Environment.GetEnvironmentVariable("DB_DATABASE");

        var connectionString =
            $"server={host};username={user};password={password};port={port};database={database};Trust Server Certificate=true;";

        optionsBuilder.UseNpgsql(connectionString);
    }

}
