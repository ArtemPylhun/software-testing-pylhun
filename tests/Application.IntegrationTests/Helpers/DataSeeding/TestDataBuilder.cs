using Bogus;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

public static class TestDataBuilder
{
    private static readonly Faker _faker = new("uk");

    public static Provider BuildProvider(
        TimeOnly start, 
        TimeOnly end, 
        string specialization = "General")
    {
        return Provider.Create(
            _faker.Name.FullName(),
            specialization,
            _faker.Internet.Email(),
            start,
            end
        );
    }

    public static Service BuildService(int durationMinutes, decimal price = 500m)
    {
        return Service.Create(
            _faker.Commerce.ProductName(),
            durationMinutes,
            price,
            _faker.Lorem.Sentence()
        );
    }

    public static Appointment BuildAppointment(
        Provider provider,
        Service service,
        DateOnly? date = null,
        TimeOnly? startTime = null)
    {
        return Appointment.Create(
            provider,
            service,
            _faker.Person.FullName,
            _faker.Internet.Email(),
            date ?? DateOnly.FromDateTime(_faker.Date.Future()),
            startTime ?? new TimeOnly(10, 0)
        );
    }
}