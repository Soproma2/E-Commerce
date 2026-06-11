using E_Commerce.Common.Pricing;
using E_Commerce.Common.Results;
using E_Commerce.Data;
using E_Commerce.DTOs.Responses;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services.Wishlist;

public class WishlistServices : IWishlistServices
{
    private readonly DataContext _context;

    public WishlistServices(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ProductResponse>>> GetWishlist(int userId)
    {
        var items = await _context.WishlistItems
            .Where(w => w.UserId == userId)
            .Include(w => w.Product)
            .ThenInclude(p => p.Category)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => w.Product)
            .ToListAsync();

        var response = items.Select(MapToList).ToList();

        return Result<List<ProductResponse>>.Ok(response);
    }

    public async Task<Result<bool>> AddToWishlist(int userId, int productId)
    {
        var alreadyInWishlist = await _context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

        if (alreadyInWishlist)
            return Result<bool>.BadRequest("Product already in wishlist.");

        var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
        if (!productExists)
            return Result<bool>.NotFound("Product not found.");

        var item = new WishlistItem
        {
            UserId = userId,
            ProductId = productId
        };

        _context.WishlistItems.Add(item);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> RemoveFromWishlist(int userId, int productId)
    {
        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (item is null)
            return Result<bool>.NotFound("Product not found in wishlist.");

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> IsInWishlist(int userId, int productId)
    {
        var exists = await _context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

        return Result<bool>.Ok(exists);
    }

    private static ProductResponse MapToList(Product p)
    {
        var discount = PriceCalculator.GetEffectiveDiscountPercent(p);

        return new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            DiscountPercent = discount > 0 ? discount : null,
            FinalPrice = PriceCalculator.GetDiscountedPrice(p.Price, discount),
            Stock = p.Stock,
            Images = p.Images == null ? null : p.Images.Take(1).ToArray(),
            Status = p.Status,
            CategoryName = p.Category.Name
        };
    }
}
