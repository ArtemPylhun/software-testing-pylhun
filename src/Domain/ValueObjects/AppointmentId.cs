namespace Domain.ValueObjects;

public readonly record struct AppointmentId(Guid Value)
{
    public static AppointmentId New() => new(Guid.NewGuid());

    public static AppointmentId Empty() => new(Guid.Empty);

    public override string ToString() => Value.ToString();
}