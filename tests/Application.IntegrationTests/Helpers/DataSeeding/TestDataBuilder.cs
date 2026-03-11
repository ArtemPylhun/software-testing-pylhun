using Bogus;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

public static class TestDataBuilder
{
    private static readonly Faker _faker = new("uk");

    public static Provider BuildProvider(string? name = null, string? email = null)
    {
        return Provider.Create(
            name ?? _faker.Name.FullName(),
            _faker.Lorem.Word(),
            email ?? _faker.Internet.Email(),
            new TimeOnly(8, 0),
            new TimeOnly(18, 0)
        );
    }

    public static Service BuildService(string? name = null, int? duration = null)
    {
        return Service.Create(
            name ?? _faker.Commerce.ProductName(),
            duration ?? _faker.Random.Int(30, 60),
            _faker.Random.Decimal(100, 1000),
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