using E_Commerce.DTOs.Requests;
using FluentValidation;

namespace E_Commerce.Validators.Cart;

public class AddToCartValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Cannot add more than 100 items at once.");
    }
}

public class EditCartItemValidator : AbstractValidator<EditCartItemRequest>
{
    public EditCartItemValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100.");
    }
}
