using Api.Helpers;
using Api.Helpers.Authorization;
using Api.Models.Problem;
using Api.Services.Abstractions;
using Core.Types;
using Core.Utilities;
using Data.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Runners.Abstractions;

namespace Api.Controllers;

[ApiController]
[Route("problems")]
public class ProblemsController : ControllerBase
{
    private readonly DirectoryInfo _tempDir = Directory.CreateDirectory(
        Path.Join(
            Path.GetTempPath(),
            "problem-tests",
            $"{DateTime.Now:yyyy-MM-dd}--{Guid.NewGuid()}"
        ));

    private readonly IProblemsService _problemsService;
    private readonly ITestableApp _testableApp;

    public ProblemsController(IProblemsService problemsService, ITestableApp testableApp)
    {
        _problemsService = problemsService;
        _testableApp = testableApp;
    }

    [HttpGet("/{programmingLanguage:required}/{solutionType:required}/problems")]
    [AttachProblemType]
    public async Task<List<ProblemResponse>> List()
    {
        var problems = await _problemsService.GetFilteredByType(HttpContext.Items["ProblemType"] as ProblemType? ?? 0)
            .Select(x => new ProblemResponse(
                x.Id, x.Title, x.Description, x.Type, x.Author.Email
            ))
            .ToListAsync();
        return problems;
    }

    [HttpGet("{id:guid:required}")]
    public async Task<ProblemResponse?> Get(Guid id)
    {
        var problem = await _problemsService.GetFilteredById(id)
            .Select(x => new ProblemResponse(x.Id, x.Title, x.Description, x.Type, x.Author.Email))
            .FirstOrDefaultAsync();
        return problem;
    }

    [Authorize(Roles = "Moderator")]
    [HttpPost("/{programmingLanguage:required}/{solutionType:required}/problems")]
    [AttachProblemType]
    public async Task<IActionResult> Create([FromForm] ProblemCreateRequest request)
    {
        var solutionDir = _tempDir.CreateSubdirectory($"{DateTime.Now:s}");

        var bytes = await Task.WhenAll(
            request.Source.OpenReadStream().CollectAsByteArrayAsync(),
            request.Input.OpenReadStream().CollectAsByteArrayAsync()
        );
        var (solution, input) = (bytes[0], bytes[1]);

        var runResult = await _testableApp.TestAsync(solutionDir, solution, input);

        if (runResult is ErrorResult<Result<string, Exception>[], Exception> errorRunResult)
        {
            return BadRequest($"Something happened at execution. Error: {errorRunResult.None.Message}");
        }

        var successRunResult = runResult as SuccessResult<Result<string, Exception>[], Exception>;
        var successResults = successRunResult!.Some
            .OfType<SuccessResult<string, Exception>>()
            .ToList();

        if (successResults.Count != successRunResult.Some.Length)
        {
            return BadRequest("Solution is not working for all the tests");
        }

        var dto = new ProblemCreateDto(
            User.GetId(),
            (ProblemType) HttpContext.Items["ProblemType"]!,
            request.Title,
            request.Description,
            input,
            new ProblemCreateSolutionDto(
                solution,
                successResults.Select(x => x.Some).ToArray()
            )
        );

        return await _problemsService.CreateAsync(dto) switch
        {
            SuccessResult<bool, Exception> => Ok(),
            ErrorResult<bool, Exception> { None: { } e } => Problem(e.Message),
            _ => Problem(),
        };
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{id:guid:required}")]
    public async Task<object> Delete(Guid id)
    {
        var problem = await _problemsService.GetFilteredById(id).FirstOrDefaultAsync();
        if (problem is null)
        {
            return NotFound();
        }

        if (problem.AuthorId != User.GetId() && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _problemsService.DeleteAsync(problem);

        return result switch
        {
            SuccessResult<bool, Exception> => Ok(),
            ErrorResult<bool, Exception> errorResult => Problem(errorResult.None.Message),
            _ => Problem(),
        };
    }
}
