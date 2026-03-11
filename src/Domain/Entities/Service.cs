using Domain.ValueObjects;

namespace Domain.Entities;

public class Service
{
    public ServiceId Id { get; private set; }
    public string Name { get; private set; }
    public int DurationMinutes { get; private set; }
    public decimal Price { get; private set; }
    public string Description { get; private set; }

    private readonly List<Provider> _providers = new();
    public IReadOnlyCollection<Provider> Providers => _providers.AsReadOnly();
    private Service()
    {
    }

    public static Service Create(
        string name,
        int durationMinutes,
        decimal price,
        string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Service's name cannot be null or whitespace", nameof(name));
        }

        if (durationMinutes <= 0)
        {
            throw new ArgumentException("Service's duration cannot be less than or equal to 0",
                nameof(durationMinutes));
        }

        if (price <= 0)
        {
            throw new ArgumentException("Service's price cannot be less than or equal to 0", nameof(price));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Service's description cannot be null or whitespace", nameof(description));
        }

        return new Service
        {
            Id = ServiceId.New(),
            Name = name,
            DurationMinutes = durationMinutes,
            Price = price,
            Description = description
        };
    }

    public void UpdateDetails(string name, int durationMinutes, decimal price, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Service's name cannot be null or whitespace", nameof(name));
        }

        if (durationMinutes <= 0)
        {
            throw new ArgumentException("Service's duration cannot be less than or equal to 0",
                nameof(durationMinutes));
        }

        if (price <= 0)
        {
            throw new ArgumentException("Service's price cannot be less than or equal to 0", nameof(price));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Service's description cannot be null or whitespace", nameof(description));
        }
        
        Name = name;
        DurationMinutes = durationMinutes;
        Price = price;
        Description = description;
    }
    
    public void AssignProvider(Provider provider)
    {
        if (!_providers.Contains(provider))
        {
            _providers.Add(provider);
        }
    }
}