using Data.Models.Common;

namespace Data.Models;

public class UserRole: BaseModel<uint>
{
    public Guid UserId { get; set; }
    public User User { get; set; }

    public Guid RoleId { get; set; }
    public Role Role { get; set; }
}
