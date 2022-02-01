using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Helpers;

[AttributeUsage(AttributeTargets.Class)]
public class AttachProblemTypeAttribute: ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var programmingLanguage = $"{context.RouteData.Values["programmingLanguage"]}".ToLower();
        var problemType = $"{context.RouteData.Values["problemType"]}".ToLower();
        if (ProblemTypeResolver.Resolve(programmingLanguage, problemType) is not { } type)
        {
            context.Result = new BadRequestObjectResult(new { message = "Unknown language or problem type" });
            return;
        }

        context.HttpContext.Items["ProblemType"] = type;
        base.OnActionExecuting(context);
    }
}
