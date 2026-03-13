using Application.Services.Commands;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Services.ValidatorTests;

public class CreateServiceCommandValidatorTests
{
    private readonly CreateServiceCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotHaveAnyErrors()
    {
        var command = new CreateServiceCommand
        {
            Name = "Consultation",
            DurationMinutes = 60,
            Price = 100.50m,
            Description = "General medical consultation"
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenFieldsAreInvalid_ShouldHaveValidationErrors()
    {
        var command = new CreateServiceCommand
        {
            Name = string.Empty,
            DurationMinutes = 0, // Не валідно
            Price = -10m,        // Не валідно
            Description = string.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
        result.ShouldHaveValidationErrorFor(x => x.Price);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}

public class DeleteServiceCommandValidatorTests
{
    private readonly DeleteServiceCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenServiceIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new DeleteServiceCommand { ServiceId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ServiceId);
    }
}

public class UpdateServiceCommandValidatorTests
{
    private readonly UpdateServiceCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenFieldsAreInvalid_ShouldHaveValidationErrors()
    {
        var command = new UpdateServiceCommand
        {
            ServiceId = Guid.Empty,
            Name = string.Empty,
            DurationMinutes = -5,
            Price = 0,
            Description = string.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ServiceId);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
        result.ShouldHaveValidationErrorFor(x => x.Price);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}