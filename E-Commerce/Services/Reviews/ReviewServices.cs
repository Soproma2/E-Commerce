using E_Commerce.Common.DTOs.Responses;
using E_Commerce.Common.Results;
using E_Commerce.Data;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;
using E_Commerce.Enums;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services.Reviews;

public class ReviewServices : IReviewServices
{
    private readonly DataContext _context;

    public ReviewServices(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<Paged<ReviewResponse>>> GetProductReviews(FilterReviewsRequest request)
    {
        var productExists = await _context.Products.AnyAsync(p => p.Id == request.ProductId);

        if (!productExists)
            return Result<Paged<ReviewResponse>>.NotFound("Product not found.");

        var query = _context.ProductReviews
            .Include(r => r.User)
            .Where(r => r.ProductId == request.ProductId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();

        var reviews = await query
            .Skip((request.Page - 1) * request.Take)
            .Take(request.Take)
            .ToListAsync();

        var items = reviews.Select(MapToResponse).ToList();

        return Result<Paged<ReviewResponse>>.Ok(
            new Paged<ReviewResponse>(items, totalCount, request.Page, request.Take));
    }

    public async Task<Result<ReviewResponse?>> GetMyReviewForProduct(int userId, int productId)
    {
        var review = await _context.ProductReviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);

        return Result<ReviewResponse?>.Ok(review is null ? null : MapToResponse(review));
    }

    public async Task<Result<ReviewResponse>> CreateReview(int userId, CreateReviewRequest request)
    {
        var product = await _context.Products.FindAsync(request.ProductId);

        if (product is null)
            return Result<ReviewResponse>.NotFound("Product not found.");

        if (product.Status != ProductStatus.Active)
            return Result<ReviewResponse>.BadRequest("Product is not available.");

        if (!await HasPurchasedProduct(userId, request.ProductId))
            return Result<ReviewResponse>.BadRequest("You can only review products you have purchased.");

        var alreadyReviewed = await _context.ProductReviews
            .AnyAsync(r => r.UserId == userId && r.ProductId == request.ProductId);

        if (alreadyReviewed)
            return Result<ReviewResponse>.BadRequest("You have already reviewed this product.");

        var review = new ProductReview
        {
            UserId = userId,
            ProductId = request.ProductId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _context.ProductReviews.Add(review);
        await _context.SaveChangesAsync();

        await _context.Entry(review).Reference(r => r.User).LoadAsync();

        return Result<ReviewResponse>.Success(201, MapToResponse(review));
    }

    public async Task<Result<ReviewResponse>> UpdateReview(int userId, int reviewId, UpdateReviewRequest request, bool isAdmin)
    {
        var review = await _context.ProductReviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review is null)
            return Result<ReviewResponse>.NotFound("Review not found.");

        if (!isAdmin && review.UserId != userId)
            return Result<ReviewResponse>.Forbidden();

        review.Rating = request.Rating;
        review.Comment = request.Comment;
        review.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result<ReviewResponse>.Ok(MapToResponse(review));
    }

    public async Task<Result<bool>> DeleteReview(int userId, int reviewId, bool isAdmin)
    {
        var review = await _context.ProductReviews.FindAsync(reviewId);

        if (review is null)
            return Result<bool>.NotFound("Review not found.");

        if (!isAdmin && review.UserId != userId)
            return Result<bool>.Forbidden();

        _context.ProductReviews.Remove(review);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    private async Task<bool> HasPurchasedProduct(int userId, int productId)
    {
        return await _context.OrderItems.AnyAsync(oi =>
            oi.ProductId == productId &&
            oi.Order.UserId == userId &&
            oi.Order.Status != OrderStatus.Cancelled);
    }

    private static ReviewResponse MapToResponse(ProductReview review) => new()
    {
        Id = review.Id,
        UserId = review.UserId,
        Username = review.User.Username,
        ProductId = review.ProductId,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAt = review.CreatedAt
    };
}
