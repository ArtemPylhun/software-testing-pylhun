using FluentValidation;

namespace Application.Appointments.Commands;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClientEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty();
    }
}