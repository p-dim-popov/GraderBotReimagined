namespace Data.Models;

public class Role
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public ICollection<UserRole> Users { get; set; }
}
