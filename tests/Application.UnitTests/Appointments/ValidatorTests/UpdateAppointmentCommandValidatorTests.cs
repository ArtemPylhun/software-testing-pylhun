using Application.Appointments.Commands;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Appointments.ValidatorTests;

public class UpdateAppointmentCommandValidatorTests
{
    private readonly UpdateAppointmentCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenFieldsAreEmpty_ShouldHaveValidationErrors()
    {
        var command = new UpdateAppointmentCommand
        {
            AppointmentId = Guid.Empty,
            ProviderId = Guid.Empty,
            ServiceId = Guid.Empty,
            Date = default,
            StartTime = default
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
        result.ShouldHaveValidationErrorFor(x => x.ProviderId);
        result.ShouldHaveValidationErrorFor(x => x.ServiceId);
        result.ShouldHaveValidationErrorFor(x => x.Date);
        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }
}