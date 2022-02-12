namespace Api.Models.Solutions;

public record SolutionAttempt(
    string Output,
    string? CorrectOutput = null
);
