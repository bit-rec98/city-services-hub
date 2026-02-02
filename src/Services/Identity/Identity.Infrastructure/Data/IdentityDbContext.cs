using Microsoft.EntityFrameworkCore;
using Identity.Domain.Entities;
using Identity.Infrastructure.Configuration;

namespace Identity.Infrastructure.Data;

/// <summary>
/// DbContext para el servicio de Identity.
/// </summary>
public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auditoría automática
        foreach (var entry in ChangeTracker.Entries<CityServicesHub.BuildingBlocks.Common.Domain.AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreatedInfo(null); // TODO: obtener usuario actual
                    break;
                case EntityState.Modified:
                    entry.Entity.SetUpdatedInfo(null);
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
