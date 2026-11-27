using CityServicesHub.BuildingBlocks.Common.Interfaces;
using MediatR;
using MedicalAppointments.Application;
using MedicalAppointments.Application.Commands;
using MedicalAppointments.Application.Behaviors;
using MedicalAppointments.Application.Validators;
using MedicalAppointments.Domain.Enums;
using MedicalAppointments.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MedicalAppointments.Tests.Application;

/// <summary>
/// Verifica la validación de entrada de los comandos de turnos, tanto las reglas como su
/// ejecución dentro del pipeline de MediatR.
/// </summary>
public class AppointmentValidationTests
{
    private static CreateAppointmentCommand ValidCreate() => new(
        PatientId: Guid.NewGuid(),
        PatientName: "Juan Pérez",
        PatientDocumentNumber: "12345678",
        PatientPhoneNumber: null,
        PatientEmail: "juan@example.com",
        DoctorId: Guid.NewGuid(),
        HealthCenterId: Guid.NewGuid(),
        Specialty: MedicalSpecialty.GeneralMedicine,
        ScheduledDate: DateTime.Today.AddDays(3),
        StartTime: new TimeOnly(10, 0),
        DurationMinutes: 30,
        AttentionType: AttentionType.InPerson,
        Reason: "Consulta");

    private readonly CreateAppointmentCommandValidator _create = new();
    private readonly CancelAppointmentCommandValidator _cancel = new();

    [Fact]
    public void Create_WithValidData_IsValid() =>
        _create.Validate(ValidCreate()).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutPatientName_IsInvalid(string name) =>
        _create.Validate(ValidCreate() with { PatientName = name }).IsValid.Should().BeFalse();

    [Fact]
    public void Create_WithTooLongPatientName_IsInvalid() =>
        _create.Validate(ValidCreate() with { PatientName = new string('a', 201) }).IsValid.Should().BeFalse();

    [Fact]
    public void Create_WithTooLongDocumentNumber_IsInvalid() =>
        _create.Validate(ValidCreate() with { PatientDocumentNumber = new string('1', 21) }).IsValid.Should().BeFalse();

    [Fact]
    public void Create_WithMalformedEmail_IsInvalid() =>
        _create.Validate(ValidCreate() with { PatientEmail = "not-an-email" }).IsValid.Should().BeFalse();

    [Fact]
    public void Create_WithoutEmail_IsValid() =>
        _create.Validate(ValidCreate() with { PatientEmail = null }).IsValid.Should().BeTrue();

    [Fact]
    public void Create_WithEmptyDoctorOrHealthCenter_IsInvalid()
    {
        _create.Validate(ValidCreate() with { DoctorId = Guid.Empty }).IsValid.Should().BeFalse();
        _create.Validate(ValidCreate() with { HealthCenterId = Guid.Empty }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Create_WithUndefinedEnumValues_IsInvalid()
    {
        _create.Validate(ValidCreate() with { Specialty = (MedicalSpecialty)9999 }).IsValid.Should().BeFalse();
        _create.Validate(ValidCreate() with { AttentionType = (AttentionType)9999 }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Create_WithPastDate_IsInvalid() =>
        _create.Validate(ValidCreate() with { ScheduledDate = DateTime.Today.AddDays(-1) }).IsValid.Should().BeFalse();

    [Fact]
    public void Create_WithTodayDate_IsValid() =>
        _create.Validate(ValidCreate() with { ScheduledDate = DateTime.Today }).IsValid.Should().BeTrue();

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(CreateAppointmentCommandValidator.MaxDurationMinutes + 1)]
    public void Create_WithDurationOutOfRange_IsInvalid(int minutes) =>
        _create.Validate(ValidCreate() with { DurationMinutes = minutes }).IsValid.Should().BeFalse();

    [Fact]
    public void Create_WithTooLongReason_IsInvalid() =>
        _create.Validate(ValidCreate() with { Reason = new string('a', 1001) }).IsValid.Should().BeFalse();

    [Fact]
    public void Cancel_WithReason_IsValid() =>
        _cancel.Validate(new CancelAppointmentCommand(Guid.NewGuid(), "Motivo")).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Cancel_WithoutReason_IsInvalid(string? reason) =>
        _cancel.Validate(new CancelAppointmentCommand(Guid.NewGuid(), reason!)).IsValid.Should().BeFalse();

    [Fact]
    public void Cancel_WithTooLongReason_IsInvalid() =>
        _cancel.Validate(new CancelAppointmentCommand(Guid.NewGuid(), new string('a', 501))).IsValid.Should().BeFalse();

    // ---- ejecución dentro del pipeline de MediatR ----

    private static (ISender Sender, Mock<IAppointmentRepository> Appointments) BuildPipeline()
    {
        var appointments = new Mock<IAppointmentRepository>();
        var user = new Mock<ICurrentUserService>();
        user.SetupGet(u => u.IsAuthenticated).Returns(true);
        user.SetupGet(u => u.UserId).Returns(Guid.NewGuid().ToString());

        var services = new ServiceCollection();
        services.AddMedicalAppointmentsApplication();
        services.AddSingleton(appointments.Object);
        services.AddSingleton(new Mock<IDoctorRepository>().Object);
        services.AddSingleton(new Mock<IHealthCenterRepository>().Object);
        services.AddSingleton(user.Object);

        return (services.BuildServiceProvider().GetRequiredService<ISender>(), appointments);
    }

    [Fact]
    public async Task Pipeline_InvalidCreate_ReturnsValidationFailureWithoutTouchingRepositories()
    {
        var (sender, appointments) = BuildPipeline();

        var result = await sender.Send(ValidCreate() with { PatientName = "", ScheduledDate = DateTime.Today.AddDays(-2) });

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ValidationBehavior<CreateAppointmentCommand, CityServicesHub.BuildingBlocks.Common.Application.Result<MedicalAppointments.Application.DTOs.AppointmentDto>>.ErrorCode);
        result.Error.Message.Should().Contain("nombre").And.Contain("pasada");
        appointments.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Pipeline_InvalidCancel_ReturnsValidationFailureWithoutTouchingRepositories()
    {
        var (sender, appointments) = BuildPipeline();

        var result = await sender.Send(new CancelAppointmentCommand(Guid.NewGuid(), ""));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation.Failed");
        appointments.VerifyNoOtherCalls();
    }
}
