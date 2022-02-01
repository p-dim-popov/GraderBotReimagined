using Data.Models.Enums;

namespace Api.Models.Problem;

public record ProblemCreateDto(
    Guid AuthorId,
    ProblemType Type,
    string Title,
    string Description,
    byte[] Source
);
