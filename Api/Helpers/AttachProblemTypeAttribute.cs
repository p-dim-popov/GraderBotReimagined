using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Helpers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AttachProblemTypeAttribute: ActionFilterAttribute
{
    public bool Skip { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // TODO: probably set order and check the one with the highest?
        var timesSkippedViaAttribute = context.ActionDescriptor.EndpointMetadata
            .OfType<AttachProblemTypeAttribute>()
            .Count(x => x.Skip);
        if (timesSkippedViaAttribute == 0)
        {
            var programmingLanguage = $"{context.RouteData.Values["programmingLanguage"]}".ToLower();
            var solutionType = $"{context.RouteData.Values["solutionType"]}".ToLower();
            if (ProblemTypeResolver.Resolve(programmingLanguage, solutionType) is not { } type)
            {
                context.Result = new BadRequestObjectResult(new { message = "Unknown language or problem type" });
                return;
            }

            context.HttpContext.Items["ProblemType"] = type;
        }

        base.OnActionExecuting(context);
    }
}
