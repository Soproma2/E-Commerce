using E_Commerce.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult ToResponse<T>(Result<T> result)
    {
        return result.Status switch
        {
            200 => Ok(result.Value),
            201 => StatusCode(201, result.Value),
            400 => BadRequest(new { result.Message, result.Errors }),
            401 => Unauthorized(new { result.Message }),
            403 => StatusCode(403, new { result.Message }),
            404 => NotFound(new { result.Message }),
            _ => StatusCode(result.Status, new { result.Message })
        };
    }

    protected int GetUserId()
    {
        return TryGetUserId(out var userId) ? userId : 0;
    }

    protected bool TryGetUserId(out int userId)
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return int.TryParse(claim?.Value, out userId);
    }

    protected IActionResult InvalidUserTokenResponse() =>
        Unauthorized(new { Message = "Invalid user token." });

    protected IActionResult ForbiddenResponse(string message) =>
        StatusCode(403, new { Message = message });
}
