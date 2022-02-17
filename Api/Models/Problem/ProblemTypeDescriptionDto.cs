using System.ComponentModel.DataAnnotations;

namespace Api.Models.Problem;

public record ProblemTypeDescriptionDto(
    [Required]
    string ProgrammingLanguage,

    [Required]
    string SolutionType
);
