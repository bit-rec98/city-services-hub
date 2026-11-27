using CityServicesHub.BuildingBlocks.Common.Interfaces;
using MedicalAppointments.Application.Commands;
using MedicalAppointments.Application.Queries;
using MedicalAppointments.Domain.Entities;
using MedicalAppointments.Domain.Enums;
using MedicalAppointments.Domain.Interfaces;

namespace MedicalAppointments.Tests.Application;

/// <summary>
/// Verifica que solo el paciente titular o un administrador puedan consultar o modificar un turno.
/// </summary>
public class AppointmentAuthorizationTests
{
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();
    private readonly Mock<IAppointmentRepository> _appointments = new();
    private readonly Mock<IDoctorRepository> _doctors = new();
    private readonly Mock<IHealthCenterRepository> _healthCenters = new();

    private static Mock<ICurrentUserService> UserWith(Guid? userId, params string[] roles)
    {
        var user = new Mock<ICurrentUserService>();
        user.SetupGet(u => u.IsAuthenticated).Returns(userId.HasValue);
        user.SetupGet(u => u.UserId).Returns(userId?.ToString());
        user.SetupGet(u => u.Roles).Returns(roles);
        user.Setup(u => u.IsInRole(It.IsAny<string>())).Returns<string>(role => roles.Contains(role));
        return user;
    }

    private Appointment ScheduledAppointmentOf(Guid patientId)
    {
        var appointment = Appointment.Create(
            patientId, "Juan Pérez", "12345678", Guid.NewGuid(), "Dr. García",
            MedicalSpecialty.GeneralMedicine, Guid.NewGuid(), "Centro 1",
            DateTime.Today.AddDays(2), new TimeOnly(10, 0), 30);

        _appointments
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        return appointment;
    }

    // ---- GET by id ----

    [Fact]
    public async Task GetById_AsOwner_ReturnsAppointment()
    {
        var appointment = ScheduledAppointmentOf(_ownerId);
        var handler = new GetAppointmentByIdQueryHandler(_appointments.Object, UserWith(_ownerId).Object);

        var result = await handler.Handle(new GetAppointmentByIdQuery(appointment.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(appointment.Id);
    }

    [Fact]
    public async Task GetById_AsOtherUser_ReturnsNotFound()
    {
        var appointment = ScheduledAppointmentOf(_ownerId);
        var handler = new GetAppointmentByIdQueryHandler(_appointments.Object, UserWith(_otherUserId, "Citizen").Object);

        var result = await handler.Handle(new GetAppointmentByIdQuery(appointment.Id), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Appointment.NotFound");
    }

    [Theory]
    [InlineData("Administrator")]
    [InlineData("SuperAdmin")]
    public async Task GetById_AsAdministrator_ReturnsAppointment(string role)
    {
        var appointment = ScheduledAppointmentOf(_ownerId);
        var handler = new GetAppointmentByIdQueryHandler(_appointments.Object, UserWith(_otherUserId, role).Object);

        var result = await handler.Handle(new GetAppointmentByIdQuery(appointment.Id), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetById_Unauthenticated_ReturnsNotFound()
    {
        var appointment = ScheduledAppointmentOf(_ownerId);
        var handler = new GetAppointmentByIdQueryHandler(_appointments.Object, UserWith(null).Object);

        var result = await handler.Handle(new GetAppointmentByIdQuery(appointment.Id), default);

        result.IsFailure.Should().BeTrue();
    }

    // ---- list by patient ----

    [Fact]
    public async Task GetPatientAppointments_ForAnotherPatient_IsForbiddenAndDoesNotQuery()
    {
        var handler = new GetPatientAppointmentsQueryHandler(_appointments.Object, UserWith(_otherUserId, "Citizen").Object);

        var result = await handler.Handle(new GetPatientAppointmentsQuery(_ownerId), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Appointment.Forbidden");
        _appointments.Verify(
            r => r.GetByPatientAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetPatientAppointments_ForSelf_ReturnsAppointments()
    {
        var appointment = ScheduledAppointmentOf(_ownerId);
        _appointments
            .Setup(r => r.GetByPatientAsync(_ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { appointment });
        var handler = new GetPatientAppointmentsQueryHandler(_appointments.Object, UserWith(_ownerId).Object);

        var result = await handler.Handle(new GetPatientAppointmentsQuery(_ownerId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
    }

    [Fact]
    public async Task GetPatientAppointments_AsAdministrator_ReturnsAppointments()
    {
        _appointments
            .Setup(r => r.GetByPatientAsync(_ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Appointment>());
        var handler = new GetPatientAppointmentsQueryHandler(
            _appointments.Object, UserWith(_otherUserId, "Administrator").Object);

        var result = await handler.Handle(new GetPatientAppointmentsQuery(_ownerId), default);

        result.IsSuccess.Should().BeTrue();
    }

    // ---- confirm ----

    [Fact]
    public async Task Confirm_AsOtherUser_ReturnsNotFoundAndDoesNotModify()
    {
        var appointment = ScheduledAppointmentOf(_ownerId);
        var handler = new ConfirmAppointmentCommandHandler(_appointments.Object, UserWith(_otherUserId, "Citizen").Object);

        var result = await handler.Handle(new ConfirmAppointmentCommand(appointment.Id), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Appointment.NotFound");
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        _appointments.Verify(
            r => r.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Confirm_AsOwner_ConfirmsAppointment()
    {
        var appointment = ScheduledAppointmentOf(_ownerId);
        var handler = new ConfirmAppointmentCommandHandler(_appointments.Object, UserWith(_ownerId).Object);

        var result = await handler.Handle(new ConfirmAppointmentCommand(appointment.Id), default);

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    // ---- cancel ----

    [Fact]
    public async Task Cancel_AsOtherUser_ReturnsNotFoundAndDoesNotModify()
    {
        var appointment = ScheduledAppointmentOf(_ownerId);
        var handler = new CancelAppointmentCommandHandler(_appointments.Object, UserWith(_otherUserId, "Citizen").Object);

        var result = await handler.Handle(new CancelAppointmentCommand(appointment.Id, "motivo"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Appointment.NotFound");
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        _appointments.Verify(
            r => r.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cancel_AsOwner_CancelsAppointment()
    {
        var appointment = ScheduledAppointmentOf(_ownerId);
        var handler = new CancelAppointmentCommandHandler(_appointments.Object, UserWith(_ownerId).Object);

        var result = await handler.Handle(new CancelAppointmentCommand(appointment.Id, "motivo"), default);

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    // ---- create ----

    private CreateAppointmentCommandHandler CreateHandler(ICurrentUserService user)
    {
        var doctor = Doctor.Create("Ana", "García", "MP-1", MedicalSpecialty.GeneralMedicine);
        var center = HealthCenter.Create("Centro 1", "C1", "Calle 1", "Córdoba", "Córdoba");
        _doctors.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
        _healthCenters.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(center);
        _appointments
            .Setup(r => r.HasConflictAsync(
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(),
                It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _appointments
            .Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment a, CancellationToken _) => a);

        return new CreateAppointmentCommandHandler(
            _appointments.Object, _doctors.Object, _healthCenters.Object, user);
    }

    private CreateAppointmentCommand CreateCommand(Guid patientIdFromBody) => new(
        patientIdFromBody, "Juan Pérez", "12345678", null, null, Guid.NewGuid(), Guid.NewGuid(),
        MedicalSpecialty.GeneralMedicine, DateTime.Today.AddDays(2), new TimeOnly(10, 0), null,
        AttentionType.InPerson, null);

    [Fact]
    public async Task Create_AsCitizen_IgnoresPatientIdFromBodyAndUsesAuthenticatedUser()
    {
        var handler = CreateHandler(UserWith(_ownerId, "Citizen").Object);

        var result = await handler.Handle(CreateCommand(patientIdFromBody: _otherUserId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be(_ownerId);
    }

    [Fact]
    public async Task Create_AsAdministrator_CanCreateForAnotherPatient()
    {
        var handler = CreateHandler(UserWith(_otherUserId, "Administrator").Object);

        var result = await handler.Handle(CreateCommand(patientIdFromBody: _ownerId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be(_ownerId);
    }

    [Fact]
    public async Task Create_Unauthenticated_Fails()
    {
        var handler = CreateHandler(UserWith(null).Object);

        var result = await handler.Handle(CreateCommand(_ownerId), default);

        result.IsFailure.Should().BeTrue();
        _appointments.Verify(
            r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
