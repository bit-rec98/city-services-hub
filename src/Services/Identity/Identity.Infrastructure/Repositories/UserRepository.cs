using Microsoft.EntityFrameworkCore;
using Identity.Domain.Entities;
using Identity.Domain.Interfaces;
using Identity.Infrastructure.Data;

namespace Identity.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de usuarios con EF Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> FindAsync(
        System.Linq.Expressions.Expression<Func<User, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Users.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
    {
        entity.SetDeletedInfo(null);
        _context.Users.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(
        System.Linq.Expressions.Expression<Func<User, bool>>? predicate = null, 
        CancellationToken cancellationToken = default)
    {
        return predicate is null 
            ? await _context.Users.CountAsync(cancellationToken)
            : await _context.Users.CountAsync(predicate, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email.Value == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<User?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default)
    {
        var normalizedDoc = documentNumber.Replace(".", "").Replace(" ", "").Trim();
        return await _context.Users
            .FirstOrDefaultAsync(u => u.DocumentNumber.Value == normalizedDoc, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.Value == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<bool> DocumentNumberExistsAsync(string documentNumber, CancellationToken cancellationToken = default)
    {
        var normalizedDoc = documentNumber.Replace(".", "").Replace(" ", "").Trim();
        return await _context.Users
            .AnyAsync(u => u.DocumentNumber.Value == normalizedDoc, cancellationToken);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(r => r.Token == refreshToken), cancellationToken);
    }
}
