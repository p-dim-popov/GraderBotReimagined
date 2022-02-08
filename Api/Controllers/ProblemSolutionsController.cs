using Api.Helpers.Authorization;
using Api.Models;
using Api.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Runners.Abstractions;

namespace Api.Controllers;

[ApiController]
[Route("solutions")]
public class ProblemSolutionsController: ControllerBase
{
    private readonly IProblemsService _problemsService;
    private readonly ITestableApp _testableApp;

    public ProblemSolutionsController(IProblemsService problemsService, ITestableApp testableApp)
    {
        _problemsService = problemsService;
        _testableApp = testableApp;
    }

    [HttpGet("{id}")]
    public dynamic GetById(string id)
    {
        return $"solution: id: {id}";
    }

    // [HttpPost("/problems/{problemId:guid:required}/solutions")]
    // public async Task<object> Submit(Guid problemId, SolutionRequest solution)
    // {
    //     var shouldSave = User.GetId() != Guid.Empty && solution.ShouldSaveResult;
    //
    //     var problem = await _problemsService.GetFilteredById(problemId)
    //         .FirstOrDefaultAsync();
    //
    //     DirectoryInfo directory = null!; // TODO: different for every input, this one is the root
    //     var results = await Task.WhenAll(
    //         problem.Input.InputValues
    //             .Select(x => _app.RunAsync(directory, x.Value))
    //         );
    //
    //     return $"{problemId}, submitted: {DateTime.Now}";
    // }
}
