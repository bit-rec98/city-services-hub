using Microsoft.EntityFrameworkCore;
using Notifications.Application.Interfaces;
using Notifications.Domain.Entities;
using Notifications.Infrastructure.Data;
using System.Linq.Expressions;

namespace Notifications.Infrastructure.Repositories;

/// <summary>
/// Implementación EF Core del repositorio de notificaciones.
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly NotificationsDbContext _context;

    public NotificationRepository(NotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Notifications.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Notification>> FindAsync(
        Expression<Func<Notification, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<Notification> AddAsync(Notification entity, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Notification entity, CancellationToken cancellationToken = default)
    {
        _context.Notifications.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Notification entity, CancellationToken cancellationToken = default)
    {
        entity.SetDeletedInfo(null);
        _context.Notifications.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications.AnyAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<Notification, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? await _context.Notifications.CountAsync(cancellationToken)
            : await _context.Notifications.CountAsync(predicate, cancellationToken);
    }

    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetByUserIdAsync(
        Guid userId,
        bool unreadOnly,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .AsQueryable();

        if (unreadOnly)
            query = query.Where(n => n.Status != NotificationStatus.Read);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && n.Status != NotificationStatus.Read, cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetPendingNotificationsAsync(
        int batchSize = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.Status == NotificationStatus.Pending)
            .OrderBy(n => n.Priority)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetDueScheduledNotificationsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Notifications
            .Where(n => n.Status == NotificationStatus.Scheduled && n.ScheduledFor <= now)
            .OrderBy(n => n.ScheduledFor)
            .ToListAsync(cancellationToken);
    }
}
