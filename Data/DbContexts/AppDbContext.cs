using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.DbContexts;

public class AppDbContext: DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Problem> Problems { get; set; }
    public DbSet<Solution> Solutions { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<UserRole> UsersRoles { get; set; }

    public DbSet<ResultValue> ResultValues { get; set; }

    public DbSet<SolutionResult> SolutionResults { get; set; }
}
