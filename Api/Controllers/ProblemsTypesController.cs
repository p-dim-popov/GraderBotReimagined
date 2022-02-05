using Api.Helpers.Authorization;
using Api.Models.Problem;
using Api.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("problems/types")]
public class ProblemsTypesController: ControllerBase
{
    private readonly IProblemsService _problemsService;

    public ProblemsTypesController(IProblemsService problemsService)
    {
        _problemsService = problemsService;
    }

    [HttpGet]
    public IEnumerable<ProblemTypeDescription> List() => _problemsService.GetAllDescriptions();

    [HttpGet("most-recent")]
    public async Task<ProblemTypeDescription> GetMostRecent() => await _problemsService.FetchMostRecentAsync(User.GetId());

}
