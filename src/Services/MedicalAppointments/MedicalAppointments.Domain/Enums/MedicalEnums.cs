namespace MedicalAppointments.Domain.Enums;

/// <summary>
/// Estado de un turno médico.
/// </summary>
public enum AppointmentStatus
{
    Scheduled = 0,    // Agendado
    Confirmed = 1,    // Confirmado
    InProgress = 2,   // En atención
    Completed = 3,    // Completado
    Cancelled = 4,    // Cancelado
    NoShow = 5,       // No se presentó
    Rescheduled = 6   // Reprogramado
}

/// <summary>
/// Especialidades médicas disponibles.
/// </summary>
public enum MedicalSpecialty
{
    GeneralMedicine = 0,      // Medicina General
    Pediatrics = 1,           // Pediatría
    Cardiology = 2,           // Cardiología
    Dermatology = 3,          // Dermatología
    Gastroenterology = 4,     // Gastroenterología
    Gynecology = 5,           // Ginecología
    Neurology = 6,            // Neurología
    Ophthalmology = 7,        // Oftalmología
    Orthopedics = 8,          // Traumatología/Ortopedia
    Otolaryngology = 9,       // Otorrinolaringología
    Psychiatry = 10,          // Psiquiatría
    Psychology = 11,          // Psicología
    Urology = 12,             // Urología
    Dentistry = 13,           // Odontología
    Endocrinology = 14,       // Endocrinología
    Pulmonology = 15,         // Neumonología
    Rheumatology = 16,        // Reumatología
    Nephrology = 17,          // Nefrología
    InfectiousDiseases = 18,  // Infectología
    EmergencyMedicine = 19,   // Urgencias
    Laboratory = 20,          // Laboratorio
    Radiology = 21            // Radiología/Imágenes
}

/// <summary>
/// Tipo de atención médica.
/// </summary>
public enum AttentionType
{
    InPerson = 0,       // Presencial
    Telemedicine = 1,   // Telemedicina
    HomeVisit = 2       // Domiciliaria
}

/// <summary>
/// Días de la semana para agendas.
/// </summary>
[Flags]
public enum DayOfWeekFlag
{
    None = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 4,
    Thursday = 8,
    Friday = 16,
    Saturday = 32,
    Sunday = 64,
    Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,
    All = Weekdays | Saturday | Sunday
}
