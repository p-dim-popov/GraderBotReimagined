using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("{programmingLanguage}/{problemType}/problems")]
public class ProblemsController: ControllerBase
{
    [HttpGet]
    public dynamic List()
    {
        return "listing";
    }

    [HttpGet("{id:required}")]
    public dynamic Get(string id)
    {
        return id;
    }
    
    [HttpPost]
    public dynamic Create()
    {
        return $"created: {DateTime.Now}";
    }

    [HttpDelete("{id:required}")]
    public dynamic Delete(string id)
    {
        return $"deleted: {DateTime.Now}";
    }
}
