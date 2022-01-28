using Microsoft.AspNetCore.Mvc;

namespace GraderBotReimagined.Controllers;

[ApiController]
[Route("javascript/single-file/problems")]
public class JavaScriptSingleFileProblemsController : Controller
{
    [HttpGet]
    public IActionResult List()
    {
        return Json("list here");
    }

    [HttpGet("{id:required}")]
    public IActionResult Get(string id)
    {
        return Json(id);
    }
    
    [HttpPost]
    public IActionResult Create()
    {
        return Json($"created: {DateTime.Now}");
    }

    [HttpDelete("{id:required}")]
    public IActionResult Delete(string id)
    {
        return Json($"deleted: {DateTime.Now}");
    }
}