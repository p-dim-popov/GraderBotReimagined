using System.ComponentModel.DataAnnotations;

namespace Api.Models.Solutions;

public record SolutionRequest([Required] IFormFile Source, bool ShouldSaveResult = false);
