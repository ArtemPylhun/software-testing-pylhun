using Application.Providers.Commands;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Providers.ValidatorTests;

public class UpdateProviderCommandValidatorTests
{
    private readonly UpdateProviderCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenFieldsAreInvalid_ShouldHaveValidationErrors()
    {
        var command = new UpdateProviderCommand
        {
            ProviderId = Guid.Empty,
            Name = string.Empty,
            Specialization = string.Empty,
            Email = "invalid-email",
            StartWorkingHours = new TimeOnly(18, 0),
            EndWorkingHours = new TimeOnly(9, 0) // End is before Start!
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ProviderId);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Specialization);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        
        // Перевіряємо правило, що EndWorkingHours має бути після StartWorkingHours
        result.ShouldHaveValidationErrorFor(x => x.EndWorkingHours)
            .WithErrorMessage("End working hours must be after start working hours.");
    }
}