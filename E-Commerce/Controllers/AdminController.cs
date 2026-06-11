using E_Commerce.Models;
using E_Commerce.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers;

[Authorize(Roles = "Admin,SalesManager")]
public class AdminController : BaseController
{
    private readonly IPromotionServices _promotionServices;
    private readonly INotificationServices _notificationServices;

    public AdminController(IPromotionServices promotionServices, INotificationServices notificationServices)
    {
        _promotionServices = promotionServices;
        _notificationServices = notificationServices;
    }

    // ─── Promotions ──────────────────────────────────────────
    [HttpGet("promotions")]
    public async Task<IActionResult> GetPromotions()
    {
        var result = await _promotionServices.GetAllPromotions();
        return ToResponse(result);
    }

    [HttpPost("promotions")]
    public async Task<IActionResult> CreatePromotion(Promotion promotion)
    {
        var result = await _promotionServices.CreatePromotion(promotion);
        return ToResponse(result);
    }

    [HttpDelete("promotions/{id}")]
    public async Task<IActionResult> DeletePromotion(int id)
    {
        var result = await _promotionServices.DeletePromotion(id);
        return ToResponse(result);
    }

    // ─── Notifications ───────────────────────────────────────
    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var result = await _notificationServices.GetNotifications();
        return ToResponse(result);
    }

    [HttpPost("notifications/{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var result = await _notificationServices.MarkAsRead(id);
        return ToResponse(result);
    }

    [HttpDelete("notifications/{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var result = await _notificationServices.DeleteNotification(id);
        return ToResponse(result);
    }
}
