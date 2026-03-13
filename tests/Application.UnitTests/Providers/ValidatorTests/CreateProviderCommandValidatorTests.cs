using Application.Providers.Commands;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Providers.ValidatorTests;

public class CreateProviderCommandValidatorTests
{
    private readonly CreateProviderCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotHaveAnyErrors()
    {
        // Arrange
        var command = new CreateProviderCommand
        {
            Name = "Dr. Gregory House",
            Specialization = "Diagnostics",
            Email = "house@princeton-plainsboro.com",
            StartWorkingHours = new TimeOnly(9, 0),
            EndWorkingHours = new TimeOnly(17, 0)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenFieldsAreEmptyOrInvalid_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new CreateProviderCommand
        {
            Name = string.Empty,
            Specialization = string.Empty,
            Email = "invalid-email-format",
            // Залишаємо час за замовчуванням (00:00)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Specialization);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WhenEndHoursAreBeforeStartHours_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateProviderCommand
        {
            Name = "Dr. Gregory House",
            Specialization = "Diagnostics",
            Email = "house@princeton-plainsboro.com",
            StartWorkingHours = new TimeOnly(18, 0),
            EndWorkingHours = new TimeOnly(9, 0) // Помилка: кінець раніше за початок
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndWorkingHours)
              .WithErrorMessage("End working hours must be after start working hours.");
    }
}