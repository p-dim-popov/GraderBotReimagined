using Microsoft.AspNetCore.Mvc;

namespace GraderBotReimagined.Controllers;

[ApiController]
[Route("{programmingLanguage}/{problemType}/problems/{problemId}/solutions")]
public class ProblemSolutionsController: Controller
{
    [HttpGet]
    public IActionResult List(string problemId)
    {
        return Json($"problemId: {problemId}");
    }

    [HttpGet("{id}")]
    public IActionResult Preview(string problemId, string id)
    {
        return Json($"problemId: {problemId}, id: {id}");
    }
    
    [HttpPost]
    public IActionResult Submit(string problemId)
    {
        return Json($"{problemId}, submitted: {DateTime.Now}");
    }
}