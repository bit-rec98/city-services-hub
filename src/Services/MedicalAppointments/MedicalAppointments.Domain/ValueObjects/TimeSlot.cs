using CityServicesHub.BuildingBlocks.Common.Domain;

namespace MedicalAppointments.Domain.ValueObjects;

/// <summary>
/// Value Object para horario de atención.
/// </summary>
public class TimeSlot : ValueObject
{
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    private TimeSlot(TimeOnly startTime, TimeOnly endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

    public static TimeSlot Create(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("La hora de inicio debe ser anterior a la hora de fin.");

        return new TimeSlot(startTime, endTime);
    }

    public static TimeSlot Create(int startHour, int startMinute, int endHour, int endMinute)
    {
        return Create(new TimeOnly(startHour, startMinute), new TimeOnly(endHour, endMinute));
    }

    public int DurationInMinutes => (int)(EndTime - StartTime).TotalMinutes;

    public bool Overlaps(TimeSlot other)
    {
        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    public bool Contains(TimeOnly time)
    {
        return time >= StartTime && time < EndTime;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartTime;
        yield return EndTime;
    }

    public override string ToString() => $"{StartTime:HH:mm} - {EndTime:HH:mm}";
}
