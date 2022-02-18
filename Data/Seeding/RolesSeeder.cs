using Data.DbContexts;
using Data.Models;
using Data.Seeding.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Data.Seeding;

public class RolesSeeder : ISeeder
{
    public async Task SeedAsync(AppDbContext context, IServiceProvider serviceProvider)
    {
        if (await context.Roles.AnyAsync())
        {
            return;
        }

        await context.Roles
            .AddRangeAsync(new Role {Name = "Admin"}, new Role {Name = "Moderator"});
    }
}
