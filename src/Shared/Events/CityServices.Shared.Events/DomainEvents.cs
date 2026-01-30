namespace CityServices.Shared.Events;

public class AppointmentCreatedEvent : DomainEvent
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Specialty { get; set; } = string.Empty;
}

public class AppointmentCancelledEvent : DomainEvent
{
    public Guid AppointmentId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class TaxPaymentCompletedEvent : DomainEvent
{
    public Guid PaymentId { get; set; }
    public Guid CitizenId { get; set; }
    public decimal Amount { get; set; }
    public string TaxType { get; set; } = string.Empty;
}

public class EmployeeHiredEvent : DomainEvent
{
    public Guid EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
}

public class LegislativeProcessStartedEvent : DomainEvent
{
    public Guid ProcessId { get; set; }
    public string ProcessType { get; set; } = string.Empty;
    public Guid InitiatorId { get; set; }
}
