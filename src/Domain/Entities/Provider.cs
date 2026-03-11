using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Provider
{
    public ProviderId Id { get; private set; }
    public string Name { get; private set; }
    public string Specialization { get; private set; }
    public string Email { get; private set; }
    public TimeOnly StartWorkingHours { get; private set; }
    public TimeOnly EndWorkingHours { get; private set; }

    private readonly List<Appointment> _appointments = new();
    public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();

    private readonly List<Service> _services = new();
    public IReadOnlyCollection<Service> Services => _services.AsReadOnly();

    private Provider()
    {
    }

    public static Provider Create(
        string name,
        string specialization,
        string email,
        TimeOnly startWorkingHours,
        TimeOnly endWorkingHours)
    {
        Validate(name, specialization, email, startWorkingHours, endWorkingHours);

        return new Provider
        {
            Id = ProviderId.New(),
            Name = name,
            Specialization = specialization,
            Email = email,
            StartWorkingHours = startWorkingHours,
            EndWorkingHours = endWorkingHours
        };
    }

    public void UpdateDetails(string name,
        string specialization,
        string email,
        TimeOnly startWorkingHours,
        TimeOnly endWorkingHours)
    {
        Validate(name,
            specialization,
            email,
            startWorkingHours,
            endWorkingHours);

        Name = name;
        Specialization = specialization;
        Email = email;
        StartWorkingHours = startWorkingHours;
        EndWorkingHours = endWorkingHours;
    }

    public void AddAppointment(Appointment appointment)
    {
        if (_appointments.Any(a => a.Date == appointment.Date &&
                                   a.Status != AppointmentStatus.Cancelled &&
                                   a.StartTime < appointment.EndTime &&
                                   a.EndTime > appointment.StartTime))
        {
            throw new InvalidOperationException("Appointment overlaps with an existing valid one.");
        }

        _appointments.Add(appointment);
    }

    public bool IsAvailable(DateOnly date, TimeOnly start, TimeOnly end)
    {
        if (start < StartWorkingHours || end > EndWorkingHours) return false;

        return !_appointments.Any(a =>
            a.Date == date &&
            a.Status != AppointmentStatus.Cancelled &&
            a.StartTime < end &&
            a.EndTime > start);
    }

    private static void Validate(string name, string specialization, string email, TimeOnly startWorkingHours,
        TimeOnly endWorkingHours)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Provider's name cannot be null or whitespace", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(specialization))
        {
            throw new ArgumentException("Provider's specialization cannot be null or whitespace",
                nameof(specialization));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Provider's email cannot be null or whitespace", nameof(email));
        }

        if (endWorkingHours <= startWorkingHours)
        {
            throw new ArgumentException("End working hours must be after start working hours.");
        }
    }
}