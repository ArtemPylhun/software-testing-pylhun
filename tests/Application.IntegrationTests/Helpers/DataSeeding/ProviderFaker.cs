using Bogus;
using Domain.Entities;

namespace Application.IntegrationTests.Helpers.DataSeeding;

public class ProviderFaker : Faker<Provider>
{
    public ProviderFaker()
    {
        CustomInstantiator(f => Provider.Create(
            f.Name.FullName(),
            f.Lorem.Word(),
            f.Internet.Email(),
            new TimeOnly(8, 0),
            new TimeOnly(18, 0)
        ));
    }
}