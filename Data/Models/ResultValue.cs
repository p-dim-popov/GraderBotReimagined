using Data.Models.Common;

namespace Data.Models;

public class ResultValue: BaseModel<uint>
{
    public string Value { get; set; }

    public bool IsSuccess { get; set; }

    public uint SolutionResultId { get; set; }

    public SolutionResult SolutionResult { get; set; }
}
