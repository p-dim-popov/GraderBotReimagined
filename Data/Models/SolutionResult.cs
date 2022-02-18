using Data.Models.Common;

namespace Data.Models;

public class SolutionResult: BaseModel<uint>
{
    public ICollection<ResultValue> ResultValues { get; set; }

    public Guid SolutionId { get; set; }

    public Solution Solution { get; set; }
}
