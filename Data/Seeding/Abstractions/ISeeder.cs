using Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Data.Seeding.Abstractions;

public interface ISeeder
{
    Task SeedAsync(AppDbContext dbContext, IServiceProvider serviceProvider);
}

public interface IEntitySeeder<T> where T: class
{
    Task SeedAsync(DbSet<T> dbSet);
}
