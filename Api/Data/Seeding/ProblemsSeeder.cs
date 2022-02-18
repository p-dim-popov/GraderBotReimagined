using System.Text;
using System.Text.Json;
using Api.Models.Problem;
using Api.Services.Abstractions;
using Data.DbContexts;
using Data.Models;
using Data.Models.Enums;
using Data.Seeding.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Api.Data.Seeding;

public class ProblemsSeeder: ISeeder
{
    public async Task SeedAsync(AppDbContext dbContext, IServiceProvider serviceProvider)
    {
        if (await dbContext.Problems.AnyAsync())
        {
            return;
        }

        var problemsService = serviceProvider.GetService<IProblemsService>() ?? throw new Exception("Problems Service is required");
        var creators = await dbContext.Users.Where(x => x.Roles.Any(y => y.Role.Name == "Moderator"))
            .ToListAsync();

        await SeedJSProblemsAsync(creators, problemsService);
    }

    private async Task SeedJSProblemsAsync(List<User> creators, IProblemsService problemsService)
    {
        var input = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new object[]
        {
            new[] {"first line", "2nd line",},
            new [] {"hello", null, " world!",},
            Array.Empty<object>(),
            new object[] { "false", true, false, },
            new object[] { 0, 1 },
        }));
        var solutionSource = Encoding.UTF8.GetBytes(@"(i) => i.forEach(x => x !== null && typeof x !== ""undefined"" && console.log(x))");
        var solutionOutputs = new []
        {
            "first line\n2nd line",
            "hello\n world!",
            "",
            "false\ntrue\nfalse",
            "0\n1"
        };

        foreach (var creator in creators)
        {
            await problemsService.CreateAsync(
                new ProblemCreateDto(
                    creator.Id,
                    ProblemType.JavaScriptSingleFileConsoleApp,
                    $"Print out the input",
                    "Just print every segment from input to the console. Skip null or undefined values",
                    input,
                    new ProblemCreateSolutionDto(solutionSource, solutionOutputs)
                )
            );
        }
    }
}
