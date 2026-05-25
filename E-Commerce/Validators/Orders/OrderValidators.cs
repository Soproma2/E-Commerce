using E_Commerce.DTOs.Requests;
using FluentValidation;

namespace E_Commerce.Validators.Orders;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required.")
            .SetValidator(new ShippingAddressValidator());

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(50).WithMessage("Payment method must not exceed 50 characters.")
            .When(x => x.PaymentMethod is not null);
    }
}

public class ShippingAddressValidator : AbstractValidator<ShippingAddressDto>
{
    public ShippingAddressValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.")
            .MaximumLength(200).WithMessage("Street must not exceed 200 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required.")
            .MaximumLength(20).WithMessage("Zip code must not exceed 20 characters.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9]{7,15}$").WithMessage("Invalid phone number format.")
            .When(x => x.PhoneNumber is not null);
    }
}

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status.");
    }
}
