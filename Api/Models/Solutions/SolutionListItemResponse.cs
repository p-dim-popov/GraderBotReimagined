using Api.Models.Problem;

namespace Api.Models.Solutions;

public record SolutionListItemResponse(
    Guid Id,
    HumanReadableTypeDescription ProblemType,
    Guid ProblemId,
    string ProblemTitle,
    DateTime CreatedOn,
    ICollection<SolutionAttempt> Attempts
)
{
    public double SuccessPercentage => Math.Round((1.0 * Attempts.Count(x => x.CorrectOutput is null) / Attempts.Count) * 100, 2);
};
