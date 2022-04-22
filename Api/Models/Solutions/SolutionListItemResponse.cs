using Api.Models.Problem;

namespace Api.Models.Solutions;

public record SolutionListItemResponse(
    Guid Id,
    HumanReadableTypeDescription ProblemType,
    Guid ProblemId,
    string ProblemTitle,
    DateTime CreatedOn,
    ICollection<SolutionAttempt> Attempts,
    string AuthorEmail
);
