using Data.Models.Common;

namespace Data.Models;

public class Role: BaseModel<Guid>
{
    public string Name { get; set; }

    public ICollection<UserRole> Users { get; set; }
}
