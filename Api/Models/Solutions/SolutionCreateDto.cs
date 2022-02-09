using Core.Types;

namespace Api.Models.Solutions;

public record SolutionCreateDto(
    Guid ProblemId,
    Guid AuthorId,
    byte[] Source,
    ICollection<SolutionCreateDto.Attempt> Result
)
{
    public record Attempt(
        string Output,
        bool IsSuccess
    );
};
