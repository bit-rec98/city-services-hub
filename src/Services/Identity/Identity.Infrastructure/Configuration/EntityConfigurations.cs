using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Identity.Domain.Entities;

namespace Identity.Infrastructure.Configuration;

/// <summary>
/// Configuración de EF Core para la entidad User.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        // Value Object Email
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();
            
            email.HasIndex(e => e.Value).IsUnique();
        });

        // Value Object DocumentNumber
        builder.OwnsOne(u => u.DocumentNumber, doc =>
        {
            doc.Property(d => d.Value)
                .HasColumnName("DocumentNumber")
                .HasMaxLength(20)
                .IsRequired();
            
            doc.HasIndex(d => d.Value).IsUnique();
        });

        builder.Property(u => u.DocumentType)
            .IsRequired();

        // Value Object PhoneNumber (opcional)
        builder.OwnsOne(u => u.PhoneNumber, phone =>
        {
            phone.Property(p => p.CountryCode)
                .HasColumnName("PhoneCountryCode")
                .HasMaxLength(5);
            
            phone.Property(p => p.AreaCode)
                .HasColumnName("PhoneAreaCode")
                .HasMaxLength(10);
            
            phone.Property(p => p.Number)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(20);
        });

        // Value Object Address (opcional)
        builder.OwnsOne(u => u.Address, addr =>
        {
            addr.Property(a => a.Street)
                .HasColumnName("AddressStreet")
                .HasMaxLength(200);
            
            addr.Property(a => a.Number)
                .HasColumnName("AddressNumber")
                .HasMaxLength(20);
            
            addr.Property(a => a.Floor)
                .HasColumnName("AddressFloor")
                .HasMaxLength(10);
            
            addr.Property(a => a.Apartment)
                .HasColumnName("AddressApartment")
                .HasMaxLength(10);
            
            addr.Property(a => a.City)
                .HasColumnName("AddressCity")
                .HasMaxLength(100);
            
            addr.Property(a => a.Province)
                .HasColumnName("AddressProvince")
                .HasMaxLength(100);
            
            addr.Property(a => a.PostalCode)
                .HasColumnName("AddressPostalCode")
                .HasMaxLength(20);
            
            addr.Property(a => a.Country)
                .HasColumnName("AddressCountry")
                .HasMaxLength(100);
        });

        builder.Property(u => u.DateOfBirth);

        builder.Property(u => u.Status)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.EmailVerified)
            .IsRequired();

        builder.Property(u => u.PhoneVerified)
            .IsRequired();

        builder.Property(u => u.TwoFactorEnabled)
            .IsRequired();

        builder.Property(u => u.TwoFactorSecret)
            .HasMaxLength(500);

        builder.Property(u => u.LastLoginAt);

        builder.Property(u => u.FailedLoginAttempts)
            .IsRequired();

        builder.Property(u => u.LockoutEnd);

        // Auditoría
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .HasMaxLength(256);

        builder.Property(u => u.UpdatedAt);

        builder.Property(u => u.UpdatedBy)
            .HasMaxLength(256);

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.DeletedAt);

        builder.Property(u => u.DeletedBy)
            .HasMaxLength(256);

        // Roles como JSON
        builder.Property("_roles")
            .HasColumnName("Roles")
            .HasColumnType("jsonb");

        // Relación con RefreshTokens
        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft delete filter
        builder.HasQueryFilter(u => !u.IsDeleted);

        // Índices
        builder.HasIndex(u => u.Status);
        builder.HasIndex(u => u.CreatedAt);
    }
}

/// <summary>
/// Configuración de EF Core para la entidad RefreshToken.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Token)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.Expires)
            .IsRequired();

        builder.Property(r => r.Created)
            .IsRequired();

        builder.Property(r => r.CreatedByIp)
            .HasMaxLength(50);

        builder.Property(r => r.Revoked);

        builder.Property(r => r.RevokedByIp)
            .HasMaxLength(50);

        builder.Property(r => r.ReplacedByToken)
            .HasMaxLength(500);

        builder.Property(r => r.ReasonRevoked)
            .HasMaxLength(500);

        builder.HasIndex(r => r.Token);
        builder.HasIndex(r => r.UserId);
    }
}
