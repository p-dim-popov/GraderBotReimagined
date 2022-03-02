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
    private readonly ITestableAppFactory _testableAppFactory;

    public ProblemsController(IProblemsService problemsService, ITestableAppFactory testableAppFactory)
    {
        _problemsService = problemsService;
        _testableAppFactory = testableAppFactory;
    }

    [HttpGet("/{programmingLanguage:required}/{solutionType:required}/problems")]
    [AttachProblemType]
    public async Task<ActionResult<List<ProblemResponse>>> List(string programmingLanguage, string solutionType)
    {
        var type = ProblemTypeResolver.Resolve(programmingLanguage, solutionType);

        var problems = type switch
        {
            { } t => await _problemsService.GetFilteredByType(t)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Description,
                    x.Type,
                    AuthorEmail = x.Author.Email
                })
                .ToListAsync(),
            _ => null,
        };

        var response = problems switch
        {
            { } p => p.Select(x => new ProblemResponse(
                x.Id,
                x.Title,
                x.Description,
                _problemsService
                    .GetAllDescriptions()
                    .First(y => y.Type == x.Type),
                x.AuthorEmail
            ))
                .ToList(),
            _ => null,
        };

        return response switch
        {
            { } => Ok(response),
            _ => NotFound(),
        };
    }

    [HttpGet("{id:guid:required}")]
    public async Task<ActionResult<ProblemResponse>> Get(Guid id)
    {
        var problem = await _problemsService.GetFilteredById(id)
            .Select(x => new {
                x.Id,
                x.Title,
                x.Description,
                x.Type,
                AuthorEmail = x.Author.Email
            })
            .FirstOrDefaultAsync();

        var response = problem switch
        {
            { } p => new ProblemResponse(
                p.Id,
                p.Title,
                p.Description,
                _problemsService
                    .GetAllDescriptions()
                    .First(y => y.Type == p.Type),
                p.AuthorEmail
            ),
            _ => null,
        };

        return response switch
        {
            { } x => Ok(x),
            _ => NotFound(),
        };
    }

    [Authorize(Roles = "Moderator")]
    [HttpPost("/{programmingLanguage:required}/{solutionType:required}/problems")]
    [AttachProblemType]
    public async Task<IActionResult> Create(string programmingLanguage, string solutionType, [FromForm] ProblemCreateRequest request)
    {
        var solutionDir = _tempDir.CreateSubdirectory($"{DateTime.Now:s}");

        var bytes = await Task.WhenAll(
            request.Source.OpenReadStream().CollectAsByteArrayAsync(),
            request.Input.OpenReadStream().CollectAsByteArrayAsync()
        );
        var (solution, input) = (bytes[0], bytes[1]);

        var testableApp = _testableAppFactory.CreateFromDescription(programmingLanguage, solutionType);
        var runResult = await testableApp.TestAsync(solutionDir, solution, input);

        if (runResult is None<Result<string, Exception>[], Exception> errorRunResult)
        {
            return BadRequest($"Something happened at execution. Error: {errorRunResult.Error.Message}");
        }

        var successRunResult = runResult as Some<Result<string, Exception>[], Exception>;
        var successResults = successRunResult!.Result
            .OfType<Some<string, Exception>>()
            .ToList();

        if (successResults.Count != successRunResult.Result.Length)
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
                successResults.Select(x => x.Result).ToArray()
            )
        );

        return await _problemsService.CreateAsync(dto) switch
        {
            Some<bool, Exception> => Ok(),
            None<bool, Exception> { Error: { } e } => Problem(e.Message),
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
            Some<bool, Exception> => Ok(),
            None<bool, Exception> errorResult => Problem(errorResult.Error.Message),
            _ => Problem(),
        };
    }
}
