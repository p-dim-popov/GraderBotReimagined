using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("{programmingLanguage}/{problemType}/problems/{problemId}/solutions")]
public class ProblemSolutionsController: ControllerBase
{
    [HttpGet]
    public dynamic List(string problemId)
    {
        return $"problemId: {problemId}";
    }

    [HttpGet("{id}")]
    public dynamic Preview(string problemId, string id)
    {
        return $"problemId: {problemId}, id: {id}";
    }
    
    [HttpPost]
    public dynamic Submit(string problemId)
    {
        return $"{problemId}, submitted: {DateTime.Now}";
    }
}