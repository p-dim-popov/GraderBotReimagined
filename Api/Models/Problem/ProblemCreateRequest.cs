using System.ComponentModel.DataAnnotations;

namespace Api.Models.Problem;

public class ProblemCreateRequest
{
    [Required] public string Title { get; set; }

    [Required] public string Description { get; set; }

    [Required] public IFormFile Source { get; set; }

    [Required] public IFormFile Input { get; set; }
}
