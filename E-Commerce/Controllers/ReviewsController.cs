using E_Commerce.DTOs.Requests;
using E_Commerce.Services.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers;

public class ReviewsController : BaseController
{
    private readonly IReviewServices _reviewServices;

    public ReviewsController(IReviewServices reviewServices)
    {
        _reviewServices = reviewServices;
    }

    [HttpGet]
    public async Task<IActionResult> GetProductReviews([FromQuery] FilterReviewsRequest request)
    {
        var result = await _reviewServices.GetProductReviews(request);
        return ToResponse(result);
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyReview([FromQuery] int productId)
    {
        if (!TryGetUserId(out var userId))
            return InvalidUserTokenResponse();

        var result = await _reviewServices.GetMyReviewForProduct(userId, productId);
        return ToResponse(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        if (!TryGetUserId(out var userId))
            return InvalidUserTokenResponse();

        var result = await _reviewServices.CreateReview(userId, request);
        return ToResponse(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewRequest request)
    {
        if (!TryGetUserId(out var userId))
            return InvalidUserTokenResponse();

        var isAdmin = User.IsInRole("Admin");
        var result = await _reviewServices.UpdateReview(userId, id, request, isAdmin);
        return ToResponse(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        if (!TryGetUserId(out var userId))
            return InvalidUserTokenResponse();

        var isAdmin = User.IsInRole("Admin");
        var result = await _reviewServices.DeleteReview(userId, id, isAdmin);
        return ToResponse(result);
    }
}
