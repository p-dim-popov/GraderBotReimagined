using Data.Models.Enums;

namespace Api.Models.Problem;

public record ProblemResponse(
    Guid Id,
    string Title,
    string Description,
    ProblemTypeDescription Type,
    string AuthorEmail
);
