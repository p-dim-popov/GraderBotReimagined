using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("solutions")]
public class ProblemSolutionsController: ControllerBase
{
    [HttpGet("{id}")]
    public dynamic GetById(string problemId, string id)
    {
        return $"problemId: {problemId}, id: {id}";
    }

    [HttpPost("/problems/{problemId:required}/solutions")]
    public dynamic Submit(string problemId)
    {
        return $"{problemId}, submitted: {DateTime.Now}";
    }
}
