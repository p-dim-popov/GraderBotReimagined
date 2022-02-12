using Api.Helpers.Authorization;
using Api.Models.Solutions;
using Api.Services.Abstractions;
using Core.Types;
using Core.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("solutions")]
public class ProblemSolutionsController: ControllerBase
{
    private readonly DirectoryInfo _tempDir = Directory.CreateDirectory(
        Path.Join(
            Path.GetTempPath(),
            "problem-tests",
            $"{DateTime.Now:yyyy-MM-dd}--{Guid.NewGuid()}"
        ));

    private readonly IProblemsService _problemsService;
    private readonly ISolutionsService _solutionsService;
    private readonly ITestableAppFactory _testableAppFactory;

    public ProblemSolutionsController(
        IProblemsService problemsService,
        ISolutionsService solutionsService,
        ITestableAppFactory testableAppFactory
    )
    {
        _problemsService = problemsService;
        _solutionsService = solutionsService;
        _testableAppFactory = testableAppFactory;
    }

    [HttpGet("{id:guid:required}")]
    public async Task<object> GetById(Guid id)
    {
        var solution = await _solutionsService.GetFilteredById(id)
            .Select(x => new
            {
                x.ProblemId,
                ProblemTitle = x.Problem.Title,
                Outputs = x.SolutionResult.ResultValues.Select(rv => rv.Value),
                CorrectOutputs = x.Problem.Solutions
                    .Where(s => s.IsAuthored)
                    .Select(s => s.SolutionResult.ResultValues
                        .Select(rv => rv.Value))
                    .First()
                    .ToList(),
                x.IsAuthored,
                x.AuthorId,
                ProblemAuthorId = x.Problem.AuthorId,
            })
            .FirstOrDefaultAsync();

        if (solution is null)
        {
            return NotFound();
        }

        if (solution.IsAuthored && solution.AuthorId != User.GetId())
        {
            return BadRequest(new { message = "Only the author can view that solution" });
        }

        var attempts = solution.Outputs
            .Select((x, i) => x == solution.CorrectOutputs[i]
                ? new SolutionAttempt(x)
                : new SolutionAttempt(x, solution.CorrectOutputs[i]));
        var result = new
        {
            solution.ProblemId,
            solution.ProblemTitle,
            solution.AuthorId,
            solution.ProblemAuthorId,
            attempts,
        };

        return result;
    }

    [HttpGet("{id:guid:required}/download")]
    public async Task<IActionResult> DownloadById(Guid id)
    {
        var solution = await _solutionsService.GetFilteredById(id)
            .Select(x => new
            {
                x.Source,
                ProblemTitle = $"{x.Problem.Title.Replace(" ", "_")}.{x.Id}",
                x.IsAuthored,
                x.AuthorId,
                ProblemAuthorId = x.Problem.AuthorId,
            })
            .FirstOrDefaultAsync();

        if (solution is null)
        {
            return NotFound();
        }

        if (solution.AuthorId != User.GetId() && solution.ProblemAuthorId != User.GetId() && !User.IsInRole("Admin"))
        {
            return BadRequest(new { message = "Only the solution author, problem author or an admin can download solutions" });
        }

        return File(solution.Source, "application/octet-stream", solution.ProblemTitle);
    }

    [AllowAnonymous]
    [HttpPost("/problems/{problemId:guid:required}/solutions")]
    public async Task<ActionResult> Submit(Guid problemId, [FromForm] SolutionRequest solution)
    {
        var shouldSave = solution.ShouldSaveResult && User.GetId() != Guid.Empty;

        var problem = await _problemsService
            .GetFilteredById(problemId)
            .Include(x => x.Solutions.Where(y => y.IsAuthored))
            .ThenInclude(x => x.SolutionResult.ResultValues)
            .FirstOrDefaultAsync();

        if (problem is null)
        {
            return NotFound();
        }

        var source = await solution.Source.OpenReadStream().CollectAsByteArrayAsync();
        var directory = _tempDir.CreateSubdirectory($"{DateTime.Now:s}");
        var testableApp = _testableAppFactory.CreateFromType(problem.Type);
        var runResult = await testableApp.TestAsync(directory, source, problem.Input);

        if (runResult is ErrorResult<Result<string, Exception>[], Exception> errorRunResult)
        {
            return Problem($"Something happened at execution. Error: {errorRunResult.None.Message}");
        }

        var successRunResult = runResult as SuccessResult<Result<string, Exception>[], Exception>;

        var correctResults = problem.Solutions.First().SolutionResult.ResultValues.ToArray();
        var results = successRunResult.Some
            .Select((x, i) => x switch
            {
                SuccessResult<string, Exception> s => new SolutionAttempt(s.Some, s.Some != correctResults[i].Value ? correctResults[i].Value : null),
                ErrorResult<string, Exception> e => new SolutionAttempt(e.None.Message, correctResults[i].Value),
                _ => new SolutionAttempt("", correctResults[i].Value),
            })
            .ToList();

        if (shouldSave)
        {
            var dto = new SolutionCreateDto(
                problemId,
                User.GetId(),
                source,
                results
            );
            await _solutionsService.SaveAsync(dto);
        }

        return Ok(results);
    }
}
