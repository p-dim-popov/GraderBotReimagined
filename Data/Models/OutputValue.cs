namespace Data.Models;

public class OutputValue
{
    public uint Id { get; set; }

    public string Value { get; set; }

    public uint SolutionOutputId { get; set; }

    public SolutionOutput SolutionOutput { get; set; }
}
