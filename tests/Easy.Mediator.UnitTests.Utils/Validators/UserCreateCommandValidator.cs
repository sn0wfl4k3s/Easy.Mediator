using Easy.Mediator.UnitTests.Utils.Requests;
using FluentValidation;

namespace Easy.Mediator.UnitTests.Utils.Validators;

public class UserCreateCommandValidator : AbstractValidator<UserCreateCommand>
{
    public UserCreateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters");
    }
}

