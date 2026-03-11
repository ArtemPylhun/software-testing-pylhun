using FluentValidation;

namespace Application.Services.Commands;

public class DeleteServiceCommandValidator : AbstractValidator<DeleteServiceCommand>
{
    public DeleteServiceCommandValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty();
    }
}