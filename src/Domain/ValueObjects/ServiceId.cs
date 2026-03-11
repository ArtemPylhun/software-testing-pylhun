namespace Domain.ValueObjects;

public readonly record struct ServiceId(Guid Value)
{
    public static ServiceId New() => new(Guid.NewGuid());

    public static ServiceId Empty() => new(Guid.Empty);

    public override string ToString() => Value.ToString();
}