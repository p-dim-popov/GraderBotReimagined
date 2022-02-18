using Data.DbContexts;
using Data.Seeding.Abstractions;
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

        foreach (var seeder in _seeders)
        {
            await seeder.SeedAsync(dbContext, serviceProvider);
            await dbContext.SaveChangesAsync();
            logger?.LogInformation($"Seeder {seeder.GetType().Name} done.");
        }
    }
}
