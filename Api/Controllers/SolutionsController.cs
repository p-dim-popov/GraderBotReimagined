using Api.Helpers;
using Api.Helpers.Authorization;
using Api.Models.Problem;
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
public class SolutionsController: ControllerBase
{
    private readonly DirectoryInfo _tempDir = Directory.CreateDirectory(
        Path.Join(
            Path.GetTempPath(),
            "problem-tests",
            $"{DateTime.Now:yyyy_MM_dd-hh_mm_ss_fff}--{Guid.NewGuid()}"
        ));

    private readonly IProblemsService _problemsService;
    private readonly ISolutionsService _solutionsService;
    private readonly ITestableAppFactory _testableAppFactory;

    public SolutionsController(
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
        var directory = _tempDir.CreateSubdirectory($"{DateTime.Now:yyyy_MM_dd-hh_mm_ss_fff}");
        var testableApp = _testableAppFactory.CreateFromType(problem.Type);
        var runResult = await testableApp.TestAsync(directory, source, problem.Input);

        if (runResult is None<Result<string, Exception>[], Exception> errorRunResult)
        {
            return Problem($"Something happened at execution. Error: {errorRunResult.Error.Message}");
        }

        var successRunResult = runResult as Some<Result<string, Exception>[], Exception>;

        var correctResults = problem.Solutions.First().SolutionResult.ResultValues.ToArray();
        var results = successRunResult.Result
            .Select((x, i) => x switch
            {
                Some<string, Exception> s => new SolutionAttempt(s.Result, s.Result != correctResults[i].Value ? correctResults[i].Value : null),
                None<string, Exception> e => new SolutionAttempt(e.Error.Message, correctResults[i].Value),
                _ => new SolutionAttempt("", correctResults[i].Value),
            })
            .ToList();

        Guid? id = shouldSave
            ? await _solutionsService.CreateAsync(new SolutionCreateDto(
                    problemId,
                    User.GetId(),
                    source,
                    results
                )) switch
                {
                    Some<Guid, Exception> { Result: { } resultId } => resultId,
                    _ => null,
                }
            : null;

        return Ok(new { id, attempts = results, });
    }

    [HttpGet]
    public async Task<object> List([FromQuery] SolutionsListItemRequest request)
    {
        var (problemTypeDescription, pagination) = request;

        var query = _solutionsService.GetAll()
            .OrderByDescending(x => x.CreatedOn)
            .Where(x => x.AuthorId == User.GetId());

        if (problemTypeDescription is var (programmingLanguage, solutionType) && ProblemTypeResolver.Resolve(programmingLanguage, solutionType) is {} type)
        {
            query = query.Where(x => x.Problem.Type == type);
        }

        if (pagination is ({} page, {} pageSize))
        {
            query = query.Skip(page * pageSize).Take(pageSize);
        }

        var list = await query
            .Select(x => new
            {
                x.Id,
                x.ProblemId,
                ProblemTitle = x.Problem.Title,
                x.Problem.Type,
                x.CreatedOn,
                Outputs = x.SolutionResult.ResultValues.Select(rv => rv.Value),
                CorrectOutputs = x.Problem.Solutions
                    .Where(s => s.IsAuthored)
                    .Select(s => s.SolutionResult.ResultValues
                        .Select(rv => rv.Value))
                    .First()
                    .ToList(),
            })
            .ToListAsync();

        var result = list
            .Select(solution => new SolutionListItemResponse(
                solution.Id,
                _problemsService.GetAllDescriptions()
                    .Where(x => x.Type == solution.Type)
                    .Select(x => new HumanReadableTypeDescription(x.DisplayName, x.Description))
                    .First(),
                solution.ProblemId,
                solution.ProblemTitle,
                solution.CreatedOn,
                solution.Outputs
                    .Select((x, i) => x == solution.CorrectOutputs[i]
                        ? new SolutionAttempt(x)
                        : new SolutionAttempt(x, solution.CorrectOutputs[i]))
                    .ToList()
            ))
            .ToList();

        return result;
    }
}
