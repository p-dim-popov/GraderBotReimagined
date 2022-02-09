using System.ComponentModel.DataAnnotations;

namespace Api.Models.Solutions;

public class SolutionRequest
{
    [Required]
    public IFormFile Source { get; set; }

    public bool ShouldSaveResult { get; set; } = false;
}
