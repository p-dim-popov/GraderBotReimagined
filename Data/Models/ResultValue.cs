namespace Data.Models;

public class ResultValue
{
    public uint Id { get; set; }

    public string Value { get; set; }

    public bool IsSuccess { get; set; }

    public uint SolutionResultId { get; set; }

    public SolutionResult SolutionResult { get; set; }
}
