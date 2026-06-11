using E_Commerce.Common.Results;
using E_Commerce.Data;
using E_Commerce.DTOs.Requests;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services.Products;

public interface IPromotionServices
{
    Task<Result<List<Promotion>>> GetAllPromotions();
    Task<Result<Promotion>> CreatePromotion(Promotion promotion);
    Task<Result<bool>> DeletePromotion(int id);
    Task<List<Promotion>> GetActivePromotions();
}

public class PromotionServices : IPromotionServices
{
    private readonly DataContext _context;

    public PromotionServices(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<Promotion>>> GetAllPromotions()
    {
        var promotions = await _context.Promotions
            .Include(p => p.Product)
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Result<List<Promotion>>.Ok(promotions);
    }

    public async Task<Result<Promotion>> CreatePromotion(Promotion promotion)
    {
        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();
        return Result<Promotion>.Ok(promotion);
    }

    public async Task<Result<bool>> DeletePromotion(int id)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion == null) return Result<bool>.NotFound("Promotion not found.");

        _context.Promotions.Remove(promotion);
        await _context.SaveChangesAsync();
        return Result<bool>.Ok(true);
    }

    public async Task<List<Promotion>> GetActivePromotions()
    {
        var now = DateTime.UtcNow;
        return await _context.Promotions
            .Where(p => p.IsEnabled && p.StartDate <= now && p.EndDate >= now)
            .ToListAsync();
    }
}
