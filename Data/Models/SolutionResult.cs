namespace Data.Models;

public class SolutionResult
{
    public uint Id { get; set; }

    public ICollection<ResultValue> ResultValues { get; set; }

    public Guid SolutionId { get; set; }

    public Solution Solution { get; set; }
}
