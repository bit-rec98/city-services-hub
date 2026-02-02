using MedicalAppointments.Domain.Entities;
using MedicalAppointments.Domain.Enums;

namespace MedicalAppointments.Tests.Domain;

/// <summary>
/// Tests unitarios para la entidad Appointment.
/// </summary>
public class AppointmentTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateAppointment()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var healthCenterId = Guid.NewGuid();
        var scheduledDate = DateTime.Today.AddDays(1);
        var startTime = new TimeOnly(10, 0);

        // Act
        var appointment = Appointment.Create(
            patientId,
            "Juan Pérez",
            "12345678",
            doctorId,
            "Dr. García",
            MedicalSpecialty.GeneralMedicine,
            healthCenterId,
            "Centro de Salud Nº 1",
            scheduledDate,
            startTime,
            20);

        // Assert
        appointment.Should().NotBeNull();
        appointment.PatientId.Should().Be(patientId);
        appointment.PatientName.Should().Be("Juan Pérez");
        appointment.DoctorId.Should().Be(doctorId);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.DurationMinutes.Should().Be(20);
        appointment.EndTime.Should().Be(startTime.AddMinutes(20));
    }

    [Fact]
    public void Create_WithPastDate_ShouldThrowException()
    {
        // Arrange
        var pastDate = DateTime.Today.AddDays(-1);

        // Act
        var act = () => Appointment.Create(
            Guid.NewGuid(),
            "Juan Pérez",
            "12345678",
            Guid.NewGuid(),
            "Dr. García",
            MedicalSpecialty.GeneralMedicine,
            Guid.NewGuid(),
            "Centro de Salud",
            pastDate,
            new TimeOnly(10, 0),
            20);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Confirm_WhenScheduled_ShouldConfirm()
    {
        // Arrange
        var appointment = CreateValidAppointment();

        // Act
        appointment.Confirm();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        appointment.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ShouldThrowException()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        appointment.Confirm();

        // Act
        var act = () => appointment.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_WhenScheduled_ShouldCancel()
    {
        // Arrange
        var appointment = CreateValidAppointment();

        // Act
        appointment.Cancel("No puedo asistir");

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancellationReason.Should().Be("No puedo asistir");
        appointment.CancelledAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldThrowException()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        appointment.Confirm();
        appointment.StartAttention();
        appointment.Complete();

        // Act
        var act = () => appointment.Cancel("Reason");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reschedule_WhenScheduled_ShouldReschedule()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        var newDate = DateTime.Today.AddDays(5);
        var newTime = new TimeOnly(14, 0);

        // Act
        appointment.Reschedule(newDate, newTime);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Rescheduled);
        appointment.ScheduledDate.Should().Be(newDate.Date);
        appointment.StartTime.Should().Be(newTime);
    }

    [Fact]
    public void Reschedule_ToPastDate_ShouldThrowException()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        var pastDate = DateTime.Today.AddDays(-1);

        // Act
        var act = () => appointment.Reschedule(pastDate, new TimeOnly(10, 0));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void StartAttention_WhenConfirmed_ShouldSetInProgress()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        appointment.Confirm();

        // Act
        appointment.StartAttention();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.InProgress);
    }

    [Fact]
    public void Complete_WhenInProgress_ShouldComplete()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        appointment.Confirm();
        appointment.StartAttention();

        // Act
        appointment.Complete("Patient seen, prescribed medication");

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.Notes.Should().Be("Patient seen, prescribed medication");
        appointment.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsNoShow_WhenScheduled_ShouldSetNoShow()
    {
        // Arrange
        var appointment = CreateValidAppointment();

        // Act
        appointment.MarkAsNoShow();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
    }

    [Fact]
    public void IsUpcoming_ForFutureScheduledAppointment_ShouldBeTrue()
    {
        // Arrange
        var appointment = CreateValidAppointment();

        // Assert
        appointment.IsUpcoming.Should().BeTrue();
    }

    [Fact]
    public void CanBeCancelled_ForScheduledAppointment_ShouldBeTrue()
    {
        // Arrange
        var appointment = CreateValidAppointment();

        // Assert
        appointment.CanBeCancelled.Should().BeTrue();
    }

    [Fact]
    public void CanBeCancelled_ForCompletedAppointment_ShouldBeFalse()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        appointment.Confirm();
        appointment.StartAttention();
        appointment.Complete();

        // Assert
        appointment.CanBeCancelled.Should().BeFalse();
    }

    private static Appointment CreateValidAppointment()
    {
        return Appointment.Create(
            Guid.NewGuid(),
            "Juan Pérez",
            "12345678",
            Guid.NewGuid(),
            "Dr. García",
            MedicalSpecialty.GeneralMedicine,
            Guid.NewGuid(),
            "Centro de Salud Nº 1",
            DateTime.Today.AddDays(1),
            new TimeOnly(10, 0),
            20);
    }
}
