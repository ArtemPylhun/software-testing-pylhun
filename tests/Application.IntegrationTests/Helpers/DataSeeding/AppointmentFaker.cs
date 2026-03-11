using Bogus;
using Domain.Entities;
using Domain.Enums;

public class AppointmentFaker : Faker<Appointment>
{
    public AppointmentFaker(Provider provider, Service service)
    {
        CustomInstantiator(f =>
        {
            var date = DateOnly.FromDateTime(f.Date.Future());
            var startTime = new TimeOnly(
                f.Random.Int(provider.StartWorkingHours.Hour, provider.EndWorkingHours.Hour - 1),
                0);

            return Appointment.Create(
                provider,
                service,
                f.Person.FullName,
                f.Internet.Email(),
                date,
                startTime
            );
        });
    }
}