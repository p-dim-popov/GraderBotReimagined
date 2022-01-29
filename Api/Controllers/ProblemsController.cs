using Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GraderBotReimagined.Controllers;

[ApiController]
public abstract class ProblemsController: Controller
{
    private IApp _app;

    protected ProblemsController(IApp app)
    {
        _app = app;
    }

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
