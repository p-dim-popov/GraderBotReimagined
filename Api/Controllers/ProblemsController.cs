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

[AttachProblemType]
[ApiController]
[Route("problems/{programmingLanguage:required}/{solutionType:required}")]
public class ProblemsController : ControllerBase
{
    private readonly IProblemsService _problemsService;

    public ProblemsController(IProblemsService problemsService)
    {
        _problemsService = problemsService;
    }

    [HttpGet]
    public async Task<object> List()
    {
        var problems = await _problemsService.SelectAllOfType(HttpContext.Items["ProblemType"] as ProblemType? ?? 0)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Description,
                x.Type,
                Author = x.Author.Email,
            })
            .ToListAsync();
        return problems;
    }

    [HttpGet("{id:required}")]
    public dynamic Get(string id)
    {
        return id;
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost]
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
