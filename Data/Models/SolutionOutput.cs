namespace Data.Models;

public class SolutionOutput
{
    public uint Id { get; set; }

    public ICollection<OutputValue> OutputValues { get; set; }

    public Guid SolutionId { get; set; }

    public Solution Solution { get; set; }
}
