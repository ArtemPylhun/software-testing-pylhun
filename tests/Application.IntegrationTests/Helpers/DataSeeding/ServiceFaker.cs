using Bogus;
using Domain.Entities;

public class ServiceFaker : Faker<Service>
{
    public ServiceFaker()
    {
        CustomInstantiator(f => Service.Create(
            f.Commerce.ProductName(),
            f.Random.Int(30, 120),
            f.Random.Decimal(100, 1000),
            f.Lorem.Sentence()
        ));
    }
}