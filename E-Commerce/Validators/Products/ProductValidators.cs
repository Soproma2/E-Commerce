using E_Commerce.DTOs.Requests;
using FluentValidation;

namespace E_Commerce.Validators.Products;

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Images)
            .Must(images => images!.Length <= 10).WithMessage("Maximum 10 images allowed.")
            .When(x => x.Images is not null);
    }
}

public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name cannot be empty.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required.")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.")
            .When(x => x.Stock.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Images)
            .Must(images => images!.Length <= 10).WithMessage("Maximum 10 images allowed.")
            .When(x => x.Images is not null);
    }
}

public class FilterProductsValidator : AbstractValidator<FilterProductsRequest>
{
    public FilterProductsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.Take)
            .InclusiveBetween(1, 100).WithMessage("Take must be between 1 and 100.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Min price cannot be negative.")
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Max price cannot be negative.")
            .GreaterThanOrEqualTo(x => x.MinPrice!.Value).WithMessage("Max price must be greater than or equal to min price.")
            .When(x => x.MaxPrice.HasValue && x.MinPrice.HasValue);
    }
}
