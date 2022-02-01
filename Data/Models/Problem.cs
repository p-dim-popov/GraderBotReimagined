using System.ComponentModel.DataAnnotations.Schema;
using Data.Models.Enums;

namespace Data.Models;

public class Problem
{
    public Guid Id { get; set; }

    public Guid AuthorId { get; set; }
    public User Author { get; set; }

    public ProblemType Type { get; set; }

    public string Title { get; set; }

    [Column(TypeName = "TEXT")]
    public string Description { get; set; }

    public byte[] Data { get; set; }
}
