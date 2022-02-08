namespace Data.Models;

public class ProblemInput
{
    public uint Id { get; set; }

    public ICollection<InputValue> InputValues { get; set; }

    public Guid ProblemId { get; set; }

    public Problem Problem { get; set; }
}
