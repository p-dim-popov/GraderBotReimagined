using Microsoft.EntityFrameworkCore;

namespace Data.DbContexts;

public class LocalDbContext: AppDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder
        .UseSqlite("data source=app.db");
}