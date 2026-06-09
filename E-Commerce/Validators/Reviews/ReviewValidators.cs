using E_Commerce.DTOs.Requests;
using FluentValidation;

namespace E_Commerce.Validators.Reviews;

public class CreateReviewValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters.");
    }
}

public class UpdateReviewValidator : AbstractValidator<UpdateReviewRequest>
{
    public UpdateReviewValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters.");
    }
}

public class FilterReviewsValidator : AbstractValidator<FilterReviewsRequest>
{
    public FilterReviewsValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product is required.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.Take)
            .InclusiveBetween(1, 100).WithMessage("Take must be between 1 and 100.");
    }
}
