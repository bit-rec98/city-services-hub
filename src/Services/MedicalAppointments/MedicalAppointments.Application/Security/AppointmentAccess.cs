using CityServicesHub.BuildingBlocks.Common.Application;
using CityServicesHub.BuildingBlocks.Common.Interfaces;
using MedicalAppointments.Domain.Entities;

namespace MedicalAppointments.Application.Security;

/// <summary>
/// Regla de autorización de turnos: un turno solo puede ser consultado o modificado por el
/// paciente titular o por un administrador. La identidad se toma siempre del token.
/// </summary>
public static class AppointmentAccess
{
    public static readonly string[] AdministratorRoles = { "Administrator", "SuperAdmin" };

    /// <summary>
    /// Respuesta para turnos ajenos. Es idéntica a "no existe" para no revelar la existencia
    /// de turnos de otros pacientes.
    /// </summary>
    public static readonly Error AppointmentNotFound =
        new("Appointment.NotFound", "El turno no fue encontrado.");

    public static readonly Error PatientForbidden =
        new("Appointment.Forbidden", "No tiene permiso para acceder a los turnos de este paciente.");

    public static readonly Error Unauthenticated =
        new("Appointment.Unauthenticated", "No se pudo identificar al usuario autenticado.");

    public static bool IsAdministrator(ICurrentUserService user) =>
        AdministratorRoles.Any(user.IsInRole);

    /// <summary>Obtiene el identificador del usuario autenticado, si es un Guid válido.</summary>
    public static bool TryGetUserId(ICurrentUserService user, out Guid userId)
    {
        userId = Guid.Empty;
        return user.IsAuthenticated && Guid.TryParse(user.UserId, out userId);
    }

    public static bool CanAccess(Appointment appointment, ICurrentUserService user)
    {
        if (!user.IsAuthenticated)
            return false;

        return IsAdministrator(user)
            || (TryGetUserId(user, out var userId) && appointment.PatientId == userId);
    }

    public static bool CanAccessPatient(Guid patientId, ICurrentUserService user)
    {
        if (!user.IsAuthenticated)
            return false;

        return IsAdministrator(user)
            || (TryGetUserId(user, out var userId) && patientId == userId);
    }
}
