using E_Commerce.Common.DTOs.Responses;
using E_Commerce.Common.Results;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;

namespace E_Commerce.Services.Reviews;

public interface IReviewServices
{
    Task<Result<Paged<ReviewResponse>>> GetProductReviews(FilterReviewsRequest request);
    Task<Result<ReviewResponse?>> GetMyReviewForProduct(int userId, int productId);
    Task<Result<ReviewResponse>> CreateReview(int userId, CreateReviewRequest request);
    Task<Result<ReviewResponse>> UpdateReview(int userId, int reviewId, UpdateReviewRequest request, bool isAdmin);
    Task<Result<bool>> DeleteReview(int userId, int reviewId, bool isAdmin);
}
