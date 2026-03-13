using Application.Providers.Commands;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Providers.ValidatorTests;

public class DeleteProviderCommandValidatorTests
{
    private readonly DeleteProviderCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenProviderIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new DeleteProviderCommand { ProviderId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProviderId);
    }
}