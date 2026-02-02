using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;
using Identity.Application.Interfaces;

namespace Identity.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de códigos de verificación usando caché distribuido.
/// </summary>
public class VerificationCodeService : IVerificationCodeService
{
    private readonly IDistributedCache _cache;

    public VerificationCodeService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public string GenerateCode(int length = 6)
    {
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        
        // Convertir a número de 6 dígitos
        var code = BitConverter.ToUInt32(bytes, 0) % (uint)Math.Pow(10, length);
        return code.ToString().PadLeft(length, '0');
    }

    public string GenerateToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_");
    }

    public async Task<bool> StoreCodeAsync(string key, string code, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        await _cache.SetStringAsync(key, code, options, cancellationToken);
        return true;
    }

    public async Task<string?> GetCodeAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _cache.GetStringAsync(key, cancellationToken);
    }

    public async Task<bool> ValidateAndRemoveCodeAsync(string key, string code, CancellationToken cancellationToken = default)
    {
        var storedCode = await _cache.GetStringAsync(key, cancellationToken);
        
        if (storedCode == null || storedCode != code)
            return false;

        await _cache.RemoveAsync(key, cancellationToken);
        return true;
    }
}
