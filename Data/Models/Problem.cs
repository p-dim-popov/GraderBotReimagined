using System.ComponentModel.DataAnnotations.Schema;
using Data.Models.Common;
using Data.Models.Enums;

namespace Data.Models;

public class Problem: BaseModel<Guid>
{
    public Guid AuthorId { get; set; }
    public User Author { get; set; }

    public ProblemType Type { get; set; }

    public string Title { get; set; }

    [Column(TypeName = "TEXT")]
    public string Description { get; set; }

    public byte[] Input { get; set; }

    public virtual ICollection<Solution> Solutions { get; set; }
}
