using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Appointment
{
    public AppointmentId Id { get; private set; }

    public string ClientName { get; private set; }
    public string ClientEmail { get; private set; }

    public DateOnly Date { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public AppointmentStatus Status { get; private set; }

    public ProviderId ProviderId { get; private set; }

    public Provider Provider { get; private set; }
    public ServiceId ServiceId { get; private set; }

    public Service Service { get; private set; }

    private Appointment()
    {
    }

    public static Appointment Create(
        Provider provider,
        Service service,
        string clientName,
        string clientEmail,
        DateOnly date,
        TimeOnly startTime)
    {
        if (string.IsNullOrWhiteSpace(clientName)) throw new ArgumentException("Client name is required");
        if (string.IsNullOrWhiteSpace(clientEmail)) throw new ArgumentException("Client email is required");

        var endTime = startTime.AddMinutes(service.DurationMinutes);

        if (endTime <= startTime)
        {
            throw new InvalidOperationException("Appointment duration crosses midnight, which is not allowed.");
        }

        if (startTime < provider.StartWorkingHours || endTime > provider.EndWorkingHours)
        {
            throw new InvalidOperationException("Appointment is outside of provider's working hours.");
        }

        return new Appointment
        {
            Id = AppointmentId.New(),
            ProviderId = provider.Id,
            ServiceId = service.Id,
            ClientName = clientName,
            ClientEmail = clientEmail,
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            Status = AppointmentStatus.Booked 
        };
    }

    public void UpdateDetails(Provider provider, Service service, DateOnly newDate, TimeOnly newStartTime)
    {
        if (Status == AppointmentStatus.Cancelled || Status == AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("Cannot update a completed or cancelled appointment.");
        }

        var newEndTime = newStartTime.AddMinutes(service.DurationMinutes);

        if (newEndTime <= newStartTime)
            throw new InvalidOperationException("Appointment duration crosses midnight, which is not allowed.");

        if (newStartTime < provider.StartWorkingHours || newEndTime > provider.EndWorkingHours)
            throw new InvalidOperationException("Appointment is outside of provider's working hours.");

        ProviderId = provider.Id;
        ServiceId = service.Id;
        Date = newDate;
        StartTime = newStartTime;
        EndTime = newEndTime;
    }
    public void Cancel(DateTime currentUtcTime)
    {
        var appointmentStart = Date.ToDateTime(StartTime);
        if (appointmentStart.Subtract(currentUtcTime).TotalHours < 2)
        {
            throw new InvalidOperationException("Cannot cancel appointment less than 2 hours before start");
        }

        Status = AppointmentStatus.Cancelled;
    }

    public void Complete()
    {
        if (Status == AppointmentStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot complete a cancelled appointment.");
        }
    
        if (Status == AppointmentStatus.Completed)
        {
            return;
        }

        Status = AppointmentStatus.Completed;
    }}