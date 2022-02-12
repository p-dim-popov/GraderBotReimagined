using Api.Helpers.Authorization;
using Api.Models.Problem;
using Api.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("problems/types")]
public class ProblemTypesController: ControllerBase
{
    private readonly IProblemsService _problemsService;

    public ProblemTypesController(IProblemsService problemsService)
    {
        _problemsService = problemsService;
    }

    [HttpGet]
    public IEnumerable<ProblemTypeDescription> List() => _problemsService.GetAllDescriptions();

    [HttpGet("most-recent")]
    public async Task<ProblemTypeDescription> GetMostRecent() => await _problemsService.FetchMostRecentAsync(User.GetId());

    [HttpGet("resolve")]
    public async Task<object?> ResolveById([FromQuery] Guid id)
    {
        var problemType = await _problemsService.GetFilteredById(id)
            .Select(x => x.Type)
            .FirstOrDefaultAsync();

        return _problemsService
            .GetAllDescriptions()
            .FirstOrDefault(x => x.Type == problemType);
    }
}
