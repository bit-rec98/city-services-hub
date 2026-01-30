namespace Identity.Domain.Enums;

/// <summary>
/// Roles del sistema para autorización.
/// </summary>
public enum UserRole
{
    Citizen = 0,           // Ciudadano común
    MunicipalEmployee = 1, // Empleado municipal
    HealthWorker = 2,      // Trabajador de salud
    TaxAgent = 3,          // Agente impositivo
    HRManager = 4,         // Gestor de RRHH
    LegislativeStaff = 5,  // Personal legislativo
    Administrator = 6,     // Administrador del sistema
    SuperAdmin = 7         // Super administrador
}

/// <summary>
/// Estado del usuario en el sistema.
/// </summary>
public enum UserStatus
{
    Pending = 0,           // Pendiente de verificación
    Active = 1,            // Activo
    Suspended = 2,         // Suspendido temporalmente
    Blocked = 3,           // Bloqueado
    Inactive = 4           // Inactivo
}

/// <summary>
/// Tipo de documento de identidad.
/// </summary>
public enum DocumentType
{
    DNI = 0,               // Documento Nacional de Identidad (Argentina)
    CUIT = 1,              // Clave Única de Identificación Tributaria
    CUIL = 2,              // Código Único de Identificación Laboral
    Passport = 3,          // Pasaporte
    ForeignId = 4          // Documento extranjero
}
