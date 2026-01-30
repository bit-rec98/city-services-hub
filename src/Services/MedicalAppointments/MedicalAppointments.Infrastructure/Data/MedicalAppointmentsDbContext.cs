using Microsoft.EntityFrameworkCore;
using MedicalAppointments.Domain.Entities;

namespace MedicalAppointments.Infrastructure.Data;

/// <summary>
/// DbContext para el servicio de Turnos Médicos.
/// </summary>
public class MedicalAppointmentsDbContext : DbContext
{
    public MedicalAppointmentsDbContext(DbContextOptions<MedicalAppointmentsDbContext> options) 
        : base(options)
    {
    }

    public DbSet<HealthCenter> HealthCenters => Set<HealthCenter>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // HealthCenter
        modelBuilder.Entity<HealthCenter>(entity =>
        {
            entity.ToTable("HealthCenters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(256);
            
            entity.OwnsOne(e => e.Location, loc =>
            {
                loc.Property(l => l.Latitude).HasColumnName("Latitude");
                loc.Property(l => l.Longitude).HasColumnName("Longitude");
            });
            
            entity.Property("_specialties").HasColumnName("Specialties").HasColumnType("jsonb");
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Doctor
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.ToTable("Doctors");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LicenseNumber).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            
            entity.Property("_secondarySpecialties").HasColumnName("SecondarySpecialties").HasColumnType("jsonb");
            entity.Property("_healthCenterIds").HasColumnName("HealthCenterIds").HasColumnType("jsonb");
            
            entity.HasMany(e => e.Schedules).WithOne().HasForeignKey(s => s.DoctorId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // DoctorSchedule
        modelBuilder.Entity<DoctorSchedule>(entity =>
        {
            entity.ToTable("DoctorSchedules");
            entity.HasKey(e => e.Id);
            
            entity.OwnsOne(e => e.TimeSlot, ts =>
            {
                ts.Property(t => t.StartTime).HasColumnName("StartTime");
                ts.Property(t => t.EndTime).HasColumnName("EndTime");
            });
        });

        // Appointment
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.PatientName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PatientDocumentNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.PatientPhoneNumber).HasMaxLength(50);
            entity.Property(e => e.PatientEmail).HasMaxLength(256);
            entity.Property(e => e.DoctorName).HasMaxLength(200);
            entity.Property(e => e.HealthCenterName).HasMaxLength(200);
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.DoctorId);
            entity.HasIndex(e => e.HealthCenterId);
            entity.HasIndex(e => e.ScheduledDate);
            entity.HasIndex(e => new { e.DoctorId, e.ScheduledDate });
            
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
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
