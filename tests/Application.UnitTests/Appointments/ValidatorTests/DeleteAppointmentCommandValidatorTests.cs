using Application.Appointments.Commands;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Appointments.ValidatorTests;

public class DeleteAppointmentCommandValidatorTests
{
    private readonly DeleteAppointmentCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenAppointmentIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new DeleteAppointmentCommand { AppointmentId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }
}