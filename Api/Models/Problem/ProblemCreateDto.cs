using Data.Models.Enums;

namespace Api.Models.Problem;

public record ProblemCreateDto(
    Guid AuthorId,
    ProblemType Type,
    string Title,
    string Description,
    byte[] Input,
    ProblemCreateSolutionDto Solution
);

public record ProblemCreateSolutionDto(
    byte[] Source,
    string[] Outputs
);
