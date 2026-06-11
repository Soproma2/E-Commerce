using E_Commerce.Common.Results;
using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services.Products;

public interface INotificationServices
{
    Task<Result<List<AdminNotification>>> GetNotifications();
    Task<Result<bool>> MarkAsRead(int id);
    Task<Result<bool>> DeleteNotification(int id);
    Task CheckLowStock(int productId, int threshold = 5);
}

public class NotificationServices : INotificationServices
{
    private readonly DataContext _context;

    public NotificationServices(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AdminNotification>>> GetNotifications()
    {
        var notifications = await _context.AdminNotifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .ToListAsync();

        return Result<List<AdminNotification>>.Ok(notifications);
    }

    public async Task<Result<bool>> MarkAsRead(int id)
    {
        var notification = await _context.AdminNotifications.FindAsync(id);
        if (notification == null) return Result<bool>.NotFound("Notification not found.");

        notification.IsRead = true;
        notification.UpdateAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> DeleteNotification(int id)
    {
        var notification = await _context.AdminNotifications.FindAsync(id);
        if (notification == null) return Result<bool>.NotFound("Notification not found.");

        _context.AdminNotifications.Remove(notification);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    public async Task CheckLowStock(int productId, int threshold = 5)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null || product.Stock > threshold) return;

        var existingNotification = await _context.AdminNotifications
            .AnyAsync(n => n.Type == "LowStock" && n.ReferenceId == productId && !n.IsRead);

        if (existingNotification) return;

        var message = $"Low stock alert: Product '{product.Name}' (ID: {product.Id}) has only {product.Stock} items left.";
        
        _context.AdminNotifications.Add(new AdminNotification
        {
            Message = message,
            Type = "LowStock",
            ReferenceId = productId,
            IsRead = false
        });

        await _context.SaveChangesAsync();
    }
}
