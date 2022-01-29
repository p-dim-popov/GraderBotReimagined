namespace Data.Models;

public class Solution
{
    public Guid Id { get; set; }

    public Guid AuthorId { get; set; }
    public User Author { get; set; }

    public Guid ProblemId { get; set; }
    public Problem Problem { get; set; }

    public byte[] Data { get; set; }
}