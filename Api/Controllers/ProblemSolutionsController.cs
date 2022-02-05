using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("problems/{programmingLanguage:required}/{solutionType:required}/{problemId}/solutions")]
public class ProblemSolutionsController: ControllerBase
{
    [HttpGet("{id}")]
    public dynamic GetById(string problemId, string id)
    {
        return $"problemId: {problemId}, id: {id}";
    }

    [HttpPost]
    public dynamic Submit(string problemId)
    {
        return $"{problemId}, submitted: {DateTime.Now}";
    }
}
