using System.Text.Json.Serialization;
using Data.Models.Common;

namespace Data.Models;

public class User: BaseModel<Guid>
{
    public string Email { get; set; }

    [JsonIgnore]
    public string Password { get; set; }

    public ICollection<UserRole> Roles { get; set; }
}
