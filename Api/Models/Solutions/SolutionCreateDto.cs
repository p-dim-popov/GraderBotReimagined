using Core.Types;

namespace Api.Models.Solutions;

public record SolutionCreateDto(
    Guid ProblemId,
    Guid AuthorId,
    byte[] Source,
    ICollection<SolutionAttempt> Result
);
