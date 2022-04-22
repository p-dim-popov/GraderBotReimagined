using System.Text.RegularExpressions;
using Api.Helpers;
using Api.Helpers.Authorization;
using Api.Models.Problem;
using Api.Services.Abstractions;
using Core.Types;
using Core.Utilities;
using Data.Models;
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

    public record ListFilters(BriefProblemTypeDescription? ProblemType, string[]? Authors);

    [HttpGet]
    public async Task<ActionResult<List<ProblemResponse>>> List([FromQuery] ListFilters? filters)
    {
        var query = _problemsService.GetAll();

        if (filters is not null)
        {
            if (filters.Authors is not null) query = query.WhereAnyMatches(x => x.Author.Email, filters.Authors);

            if (filters.ProblemType is not null)
            {
                var type = ProblemTypeResolver.Resolve(filters.ProblemType.ProgrammingLanguage, filters.ProblemType.SolutionType);
                query = query.Where(x => x.Type == type);
            }
        }

        var problems = await query
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Description,
                x.Type,
                AuthorEmail = x.Author.Email
            })
            .ToListAsync();

        var response = problems
            .Select(x => new ProblemResponse(
                x.Id,
                x.Title,
                x.Description,
                _problemsService
                    .GetAllDescriptions()
                    .First(y => y.Type == x.Type),
                x.AuthorEmail
            ))
            .ToList();

        return Ok(response);
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
        var dtoResult = await TryCreateDto(
            programmingLanguage, solutionType,
            request.Title, request.Description,
            (request.Input.OpenReadStream().CollectAsByteArrayAsync(), request.Source.OpenReadStream().CollectAsByteArrayAsync())
        );

        if (dtoResult is None<ProblemEditDto, IActionResult> { Error: { } error }) return error;
        if (dtoResult is not Some<ProblemEditDto, IActionResult> { Result: {} dto}) return BadRequest(new { message = "Something went wrong" });
        var createDto = new ProblemCreateDto(
            dto.AuthorId,
            dto.Type,
            dto.Title, dto.Description,
            dto.Input!, dto.Solution!
        );

        return await _problemsService.CreateAsync(createDto) switch
        {
            Some<Problem, Exception> { Result: { } entity } => Ok(new
            {
                entity.Id,
            }),
            None<Problem, Exception> { Error: { } e } => Problem(e.Message),
            _ => Problem(),
        };
    }

    [Authorize(Roles = "Moderator")]
    [HttpPatch("/{programmingLanguage:required}/{solutionType:required}/problems")]
    [AttachProblemType]
    public async Task<IActionResult> Edit(string programmingLanguage, string solutionType, [FromForm] ProblemEditRequest request)
    {
        var originalQuery = _problemsService.GetFilteredById(request.Id);
        bool hasChangedSource = request.Source is not null, hasChangedInput = request.Input is not null;
        var shouldTest = hasChangedInput || hasChangedSource;

        if (hasChangedInput && !hasChangedSource)
        {
            originalQuery = originalQuery
                .Include(problem => problem.Solutions.Where(y => y.IsAuthored));
        }

        var original = await originalQuery.FirstAsync();
        var dtoResult = await TryCreateDto(
            programmingLanguage, solutionType,
            request.Title, request.Description,
            shouldTest ?
            (
                hasChangedInput
                    ? request.Input!.OpenReadStream().CollectAsByteArrayAsync()
                    : Task.FromResult(original.Input),
                hasChangedSource
                    ? request.Source!.OpenReadStream().CollectAsByteArrayAsync()
                    : Task.FromResult(original.Solutions.First().Source)
            ) : null
        );
        if (dtoResult is None<ProblemEditDto, IActionResult> { Error: { } error }) return error;
        if (dtoResult is not Some<ProblemEditDto, IActionResult> { Result: {} dto}) return BadRequest(new { message = "Something went wrong" });

        var editDto = new ProblemEditDto(
            request.Id, dto.AuthorId,
            dto.Type,
            dto.Title, dto.Description,
            dto.Input, dto.Solution
        );
        return await _problemsService.EditAsync(editDto, User.IsInRole("Admin")) switch
        {
            Some<Problem, Exception> => Ok(new { request.Id }),
            None<Problem, Exception> { Error: { } e } => Problem(e.Message),
            _ => Problem(),
        };
    }

    private async Task<Result<ProblemEditDto, IActionResult>> TryCreateDto(
        string programmingLanguage, string solutionType,
        string title, string description,
        (Task<byte[]> input, Task<byte[]> source)? codeTasks
    )
    {
        byte[]? input = null;
        ProblemCreateSolutionDto? solution = null;
        if (codeTasks is { input: { } inputBytesTask, source: {} sourceBytesTask })
        {
            var bytes = await Task.WhenAll(sourceBytesTask, inputBytesTask);
            var solutionBytes = bytes[0];
            input = bytes[1];

            var solutionDir = _tempDir.CreateSubdirectory($"{DateTime.Now:s}");

            var testableApp = _testableAppFactory.CreateFromDescription(programmingLanguage, solutionType);
            var runResult = await testableApp.TestAsync(solutionDir, solutionBytes, input);

            if (runResult is None<Result<string, Exception>[], Exception> errorRunResult)
            {
                return new None<ProblemEditDto, IActionResult>(BadRequest(new { message = $"Something happened at execution. Error: {errorRunResult.Error.Message}" }));
            }

            var successRunResult = runResult as Some<Result<string, Exception>[], Exception>;
            var successResults = successRunResult!.Result
                .OfType<Some<string, Exception>>()
                .ToList();

            if (successResults.Count != successRunResult.Result.Length)
            {
                return new None<ProblemEditDto, IActionResult>(BadRequest(new { message = "Solution is not working for all the tests" }));
            }

            solution = new ProblemCreateSolutionDto(
                solutionBytes,
                successResults.Select(x => x.Result).ToArray()
            );
        }

        var dto = new ProblemEditDto(
            Guid.Empty,
            User.GetId(),
            (ProblemType) HttpContext.Items["ProblemType"]!,
            title,
            description,
            input,
            solution
        );

        return new Some<ProblemEditDto, IActionResult>(dto);
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
