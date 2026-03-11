using FluentValidation;

namespace Application.Providers.Commands;

public class UpdateProviderCommandValidator : AbstractValidator<UpdateProviderCommand>
{
    public UpdateProviderCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.StartWorkingHours).NotEmpty();
        RuleFor(x => x.EndWorkingHours).NotEmpty()
            .GreaterThan(x => x.StartWorkingHours).WithMessage("End working hours must be after start working hours.");
    }
}