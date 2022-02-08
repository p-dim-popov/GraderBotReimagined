namespace Data.Models;

public class InputValue
{
    public uint Id { get; set; }

    public string Value { get; set; }

    public uint ProblemInputId { get; set; }

    public ProblemInput ProblemInput { get; set; }
}
