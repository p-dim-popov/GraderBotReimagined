using System.Text;
using Api.Models.Solutions;
using Api.Services.Abstractions;
using Data.DbContexts;
using Data.Models;
using Data.Seeding.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Api.Data.Seeding;

public class SolutionsSeeder: ISeeder
{
    public async Task SeedAsync(AppDbContext dbContext, IServiceProvider serviceProvider)
    {
        var users = await dbContext.Users.ToListAsync();
        await new ProblemPrintOutTheInputSolutionSeeder(users, serviceProvider).SeedAsync(dbContext.Solutions);
    }

    private class ProblemPrintOutTheInputSolutionSeeder: IEntitySeeder<Solution>
    {
        private readonly List<User> _users;
        private readonly IServiceProvider _serviceProvider;

        public ProblemPrintOutTheInputSolutionSeeder(List<User> users, IServiceProvider serviceProvider)
        {
            _users = users;
            _serviceProvider = serviceProvider;
        }

        public async Task SeedAsync(DbSet<Solution> dbSet)
        {
            if (await dbSet.AnyAsync(x => x.Problem.Title.ToLower() == "print out the input" && !x.IsAuthored))
            {
                return;
            }

            var problem = await dbSet
                .Where(x => x.Problem.Title.ToLower().Contains("print out the input"))
                .Select(x => x.Problem)
                .FirstAsync();

            var solutionsService = _serviceProvider.GetService<ISolutionsService>() ?? throw new Exception("Solutions service is required");
            var solutionSource = Encoding.UTF8.GetBytes("(x) => {for(let y of x) if (y) console.log(y)}");

            foreach (var user in _users)
            {
                await solutionsService.CreateAsync(new SolutionCreateDto(
                    problem.Id,
                    user.Id,
                    solutionSource,
                    new List<SolutionAttempt>
                    {
                        new("first line\n2nd line"),
                        new("hello\n world!"),
                        new(""),
                        new("false\ntrue", "false\ntrue\nfalse"),
                        new("1", "0\n1"),
                    }
                ));
            }
        }
    }
}
