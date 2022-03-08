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

        await Task.WhenAll(
            SeedJsProblemsAsync(creators, problemsService),
            SeedCsProblemsAsync(creators, problemsService)
        );
    }

    private async Task SeedCsProblemsAsync(List<User> creators, IProblemsService problemsService)
    {
        var input = new []
        {
            new[] {"1", "3", "3", "7", "END"},
            new [] {"1.337", "END"},
            new [] {"1", "2", "hehe", "END"},
            new [] {"1", "", "3", "END"},
        };

        const string solutionSource = @"
            var input = Console.ReadLine();
            while (input != ""END"")
            {
                if (!string.IsNullOrWhiteSpace(input))
                    if (double.TryParse(input, out var number))
                        Console.WriteLine(number);

                input = Console.ReadLine();
            }
            ";
        var solutionOutputs = new []
        {
            "1\n3\n3\n7",
            "1.337",
            "1\n2",
            "1\n3",
        };

        await SeedProblemAsync(
            creators,
            problemsService,
            ProblemType.CSharpSingleFileConsoleApp,
            "Print the numbers",
            "Print every number from the input.\nCaution! Some input values may not be numbers, some are floating point numbers.\nRead until you reach input 'END'",
            input,
            solutionSource,
            solutionOutputs
        );
    }

    private async Task SeedJsProblemsAsync(List<User> creators, IProblemsService problemsService)
    {
        var input = new []
        {
            new[] {"first line", "2nd line",},
            new [] {"hello", null, " world!",},
            Array.Empty<object>(),
            new object[] { "false", true, false, },
            new object[] { 0, 1 },
        };
        const string solutionSource = @"(i) => i.forEach(x => x !== null && typeof x !== ""undefined"" && console.log(x))";
        var solutionOutputs = new []
        {
            "first line\n2nd line",
            "hello\n world!",
            "",
            "false\ntrue\nfalse",
            "0\n1"
        };

        await SeedProblemAsync(
            creators,
            problemsService,
            ProblemType.JavaScriptSingleFileConsoleApp,
            "Print out the input",
            "Just print every segment from input to the console. Skip null or undefined values",
            input,
            solutionSource,
            solutionOutputs
        );
    }

    private async Task SeedProblemAsync(List<User> creators, IProblemsService problemsService, ProblemType type, string title, string description, object[][] inputs, string solutionSource, string[] outputs)
    {
        var serializedInputs = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(inputs));
        var solution = Encoding.UTF8.GetBytes(solutionSource);

        foreach (var creator in creators)
        {
            await problemsService.CreateAsync(
                new ProblemCreateDto(
                    creator.Id,
                    type,
                    title,
                    description,
                    serializedInputs,
                    new ProblemCreateSolutionDto(solution, outputs)
                )
            );
        }
    }
}
