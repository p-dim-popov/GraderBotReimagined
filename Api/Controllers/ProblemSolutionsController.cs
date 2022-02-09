using Api.Helpers.Authorization;
using Api.Models;
using Api.Models.Solutions;
using Api.Services.Abstractions;
using Core.Types;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Runners.Abstractions;

namespace Api.Controllers;

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

    [HttpGet("{id}")]
    public dynamic GetById(string id)
    {
        return $"solution: id: {id}";
    }

    [HttpPost("/problems/{problemId:guid:required}/solutions")]
    public async Task<ActionResult> Submit(Guid problemId, [FromForm] SolutionRequest solution)
    {
        var shouldSave = solution.ShouldSaveResult && User.GetId() != Guid.Empty;

        var problem = await _problemsService
            .GetFilteredById(problemId)
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

        var results = successRunResult.Some
            .Select(x => x switch
            {
                SuccessResult<string, Exception> s => new SolutionCreateDto.Attempt(s.Some, true),
                ErrorResult<string, Exception> e => new SolutionCreateDto.Attempt(e.None.Message, false),
                _ => new SolutionCreateDto.Attempt("", false),
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
