using E_Commerce.DTOs.Requests;
using FluentValidation;

namespace E_Commerce.Validators.Categories;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.ParentId)
            .GreaterThan(0).WithMessage("Parent category ID must be greater than 0.")
            .When(x => x.ParentId.HasValue);
    }
}

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name cannot be empty.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.ParentId)
            .GreaterThan(0).WithMessage("Parent category ID must be greater than 0.")
            .When(x => x.ParentId.HasValue);
    }
}
