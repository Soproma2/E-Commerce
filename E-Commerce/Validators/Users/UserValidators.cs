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
    }
}
