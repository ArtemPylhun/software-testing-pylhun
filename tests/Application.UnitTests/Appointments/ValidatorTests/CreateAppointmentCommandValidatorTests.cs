using Application.Appointments.Commands;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Appointments.ValidatorTests;

public class CreateAppointmentCommandValidatorTests
{
    private readonly CreateAppointmentCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenFieldsAreEmpty_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new CreateAppointmentCommand
        {
            ProviderId = Guid.Empty,
            ServiceId = Guid.Empty,
            ClientName = string.Empty,
            ClientEmail = "not-an-email", // Невалідний email
            Date = default,
            StartTime = default
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProviderId);
        result.ShouldHaveValidationErrorFor(x => x.ServiceId);
        result.ShouldHaveValidationErrorFor(x => x.ClientName);
        result.ShouldHaveValidationErrorFor(x => x.ClientEmail);
        result.ShouldHaveValidationErrorFor(x => x.Date);
        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void Validate_WhenFieldsAreValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateAppointmentCommand
        {
            ProviderId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            ClientName = "John Doe",
            ClientEmail = "john.doe@example.com",
            Date = new DateOnly(2024, 12, 1),
            StartTime = new TimeOnly(10, 0)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}