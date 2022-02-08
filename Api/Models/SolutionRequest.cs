using Microsoft.AspNetCore.Mvc;

namespace Api.Models;

public class SolutionRequest
{
    [FromForm]
    public byte[] Source { get; set; }

    public bool ShouldSaveResult { get; set; }
}
