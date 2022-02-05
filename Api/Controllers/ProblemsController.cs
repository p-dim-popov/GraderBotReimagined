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
    private readonly IProblemsService _problemsService;

    public ProblemsController(IProblemsService problemsService)
    {
        _problemsService = problemsService;
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

    [Authorize(Roles = "Administrator")]
    [HttpPost("/{programmingLanguage:required}/{solutionType:required}/problems")]
    [AttachProblemType]
    public async Task<IActionResult> Create([FromForm] ProblemCreateRequest request)
    {
        var dto = new ProblemCreateDto(
            User.GetId(),
            (ProblemType) HttpContext.Items["ProblemType"]!,
            request.Title,
            request.Description,
            await request.Source.OpenReadStream().CollectAsByteArrayAsync()
        );

        return await _problemsService.CreateAsync(dto) switch
        {
            SuccessResult<bool, Exception> => Ok(),
            ErrorResult<bool, Exception> { None: { } e } => Problem(e.Message),
            _ => Problem(),
        };
    }

    [Authorize(Roles = "Administrator")]
    [HttpDelete("{id:required}")]
    public dynamic Delete(string id)
    {
        return $"deleted: {DateTime.Now}";
    }
}
