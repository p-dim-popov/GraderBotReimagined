using Data.Models.Common;

namespace Data.Models;

public class Solution: BaseModel<Guid>
{
    public Guid AuthorId { get; set; }
    public User Author { get; set; }

    public Guid ProblemId { get; set; }
    public Problem Problem { get; set; }

    public byte[] Source { get; set; }

    public SolutionResult SolutionResult { get; set; }

    public bool IsAuthored { get; set; }
}
