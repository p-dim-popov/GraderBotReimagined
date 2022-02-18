using Data.Models;
using Data.Models.Common;
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

    public override int SaveChanges() => SaveChanges(true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditInfoRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(true, cancellationToken);

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditInfoRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditInfoRules()
    {
        var changedEntries = ChangeTracker
            .Entries()
            .Where(e =>
                e.Entity is IAuditInfo &&
                e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in changedEntries)
        {
            if (entry.Entity is not IAuditInfo entity)
            {
                return;
            }

            if (entry.State == EntityState.Added && entity.CreatedOn == default)
            {
                entity.CreatedOn = DateTime.UtcNow;
            }
            else
            {
                entity.ModifiedOn = DateTime.UtcNow;
            }
        }
    }
}
