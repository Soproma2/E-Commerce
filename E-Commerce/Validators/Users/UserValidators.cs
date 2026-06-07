using E_Commerce.DTOs.Requests;
using FluentValidation;

namespace E_Commerce.Validators.Users;

public class EditUserValidator : AbstractValidator<EditUserRequest>
{
    public EditUserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name cannot be empty.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name cannot be empty.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.")
            .When(x => x.LastName is not null);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9]{7,15}$").WithMessage("Invalid phone number format.")
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address cannot be empty.")
            .MaximumLength(300).WithMessage("Address must not exceed 300 characters.")
            .When(x => x.Address is not null);

        RuleFor(x => x)
            .Must(x => !(x.ClearFirstName && x.FirstName is not null))
            .WithMessage("First name cannot be set and cleared at the same time.");

        RuleFor(x => x)
            .Must(x => !(x.ClearLastName && x.LastName is not null))
            .WithMessage("Last name cannot be set and cleared at the same time.");

        RuleFor(x => x)
            .Must(x => !(x.ClearPhoneNumber && x.PhoneNumber is not null))
            .WithMessage("Phone number cannot be set and cleared at the same time.");

        RuleFor(x => x)
            .Must(x => !(x.ClearAddress && x.Address is not null))
            .WithMessage("Address cannot be set and cleared at the same time.");
    }
}

public class TopUpBalanceValidator : AbstractValidator<TopUpBalanceRequest>
{
    public TopUpBalanceValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Top-up amount must be greater than 0.")
            .LessThanOrEqualTo(10000).WithMessage("Top-up amount cannot exceed 10000.");
    }
}
