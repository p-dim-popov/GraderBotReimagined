using Contracts;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace GraderBotReimagined.Controllers;

[Route("javascript/single-file/problems")]
public class JavaScriptSingleFileProblemsController : BaseController
{
    public JavaScriptSingleFileProblemsController(IProcessStarter processStarter) : base(new JavaScriptSingleFileConsoleApp(processStarter))
    { }
}