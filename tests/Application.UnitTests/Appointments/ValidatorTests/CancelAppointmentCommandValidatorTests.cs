using Application.Appointments.Commands;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Appointments.ValidatorTests;

public class CancelAppointmentCommandValidatorTests
{
    private readonly CancelAppointmentCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenAppointmentIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CancelAppointmentCommand { AppointmentId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Fact]
    public void Validate_WhenAppointmentIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CancelAppointmentCommand { AppointmentId = Guid.NewGuid() };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AppointmentId);
    }
}