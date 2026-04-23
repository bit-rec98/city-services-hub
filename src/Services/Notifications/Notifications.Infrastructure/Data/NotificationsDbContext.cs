using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Entities;
using Notifications.Infrastructure.Configuration;

namespace Notifications.Infrastructure.Data;

/// <summary>
/// DbContext para el servicio de Notifications.
/// </summary>
public class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options)
    {
    }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<CityServicesHub.BuildingBlocks.Common.Domain.AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreatedInfo(null);
                    break;
                case EntityState.Modified:
                    entry.Entity.SetUpdatedInfo(null);
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
