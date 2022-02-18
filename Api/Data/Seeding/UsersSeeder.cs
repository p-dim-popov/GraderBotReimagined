using Data.DbContexts;
using Data.Models;
using Data.Seeding.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Api.Data.Seeding;

public class UsersSeeder : ISeeder
{
    public async Task SeedAsync(AppDbContext context, IServiceProvider serviceProvider)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        // var configuration = serviceProvider.GetService<IConfiguration>();
        const string password = "123"; // configuration?.GetSection("BuiltInProfilesPassword").Value;
        var roles = await context.Roles.ToListAsync();

        await context.Users.AddRangeAsync(
            new User
            {
                Email = "admin@local.host",
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Roles = roles.Select(x => new UserRole { Role = x }).ToList(),
            },
            new User
            {
                Email = "moderator@local.host",
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Roles = roles
                    .Where(x => x.Name == "Moderator")
                    .Select(x => new UserRole { Role = x })
                    .ToList(),
            },
            new User
            {
                Email = "not-admin@local.host",
                Password = BCrypt.Net.BCrypt.HashPassword(password),
            }
        );
    }
}
