namespace Domain.ValueObjects;

public readonly record struct ProviderId(Guid Value)
{
    public static ProviderId New() => new(Guid.NewGuid());

    public static ProviderId Empty() => new(Guid.Empty);

    public override string ToString() => Value.ToString();
}