using Data.DbContexts;
using Data.Seeding.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Data.Seeding;

public class AppDbSeeder : ISeeder
{
    private readonly IEnumerable<ISeeder> _seeders;

    public AppDbSeeder(IEnumerable<ISeeder> seeders)
    {
        _seeders = seeders;
    }

    public async Task SeedAsync(AppDbContext dbContext, IServiceProvider serviceProvider)
    {
        if (dbContext == null)
        {
            throw new ArgumentNullException(nameof(dbContext));
        }

        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(AppDbContext));

        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Database.MigrateAsync();

        foreach (var seeder in _seeders)
        {
            await seeder.SeedAsync(dbContext, serviceProvider);
            await dbContext.SaveChangesAsync();
            logger?.LogInformation($"Seeder {seeder.GetType().Name} done.");
        }
    }
}
