using FluentValidation;

namespace Application.Appointments.Commands;

public class DeleteAppointmentCommandValidator : AbstractValidator<DeleteAppointmentCommand>
{
    public DeleteAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
    }
}