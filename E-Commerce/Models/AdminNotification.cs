using E_Commerce.Common.Entities;

namespace E_Commerce.Models;

public class AdminNotification : Entity
{
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "General"; // LowStock, Order, System
    public bool IsRead { get; set; }
    public int? ReferenceId { get; set; }
}
