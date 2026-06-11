using E_Commerce.Common.DTOs.Responses;
using E_Commerce.Common.Pricing;
using E_Commerce.Common.Results;
using E_Commerce.Data;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;
using E_Commerce.Enums;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services.Products;

public class ProductServices : IProductServices
{
    private readonly DataContext _context;
    private readonly INotificationServices _notificationServices;
    private readonly IPromotionServices _promotionServices;

    public ProductServices(DataContext context, INotificationServices notificationServices, IPromotionServices promotionServices)
    {
        _context = context;
        _notificationServices = notificationServices;
        _promotionServices = promotionServices;
    }

    public async Task<Result<Paged<ProductResponse>>> GetProducts(FilterProductsRequest request)
    {
        var activePromotions = await _promotionServices.GetActivePromotions();
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Name.Contains(request.Search));

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice);

        query = request.Status.HasValue
            ? query.Where(p => p.Status == request.Status)
            : query.Where(p => p.Status == ProductStatus.Active);

        var totalCount = await query.CountAsync();

        var products = await query
            .Skip((request.Page - 1) * request.Take)
            .Take(request.Take)
            .ToListAsync();

        var items = products.Select(p => MapToList(p, activePromotions)).ToList();

        return Result<Paged<ProductResponse>>.Ok(new Paged<ProductResponse>(items, totalCount, request.Page, request.Take));
    }

    public async Task<Result<ProductDetailsResponse>> GetProductById(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return Result<ProductDetailsResponse>.NotFound("Product not found.");

        var activePromotions = await _promotionServices.GetActivePromotions();
        var reviewStats = await GetReviewStats(id);

        return Result<ProductDetailsResponse>.Ok(MapToDetails(
            product,
            reviewStats.reviewCount,
            reviewStats.averageRating,
            activePromotions));
    }

    public async Task<Result<ProductDetailsResponse>> CreateProduct(CreateProductRequest request)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
            return Result<ProductDetailsResponse>.NotFound("Category not found.");

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CategoryId = request.CategoryId,
            Stock = request.Stock,
            Images = request.Images,
            Status = request.Status,
            DiscountPercent = request.DiscountPercent
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        await _notificationServices.CheckLowStock(product.Id);

        return Result<ProductDetailsResponse>.Success(201, MapToDetails(product, 0, null, new List<Promotion>()));
    }

    public async Task<Result<ProductDetailsResponse>> UpdateProduct(int id, UpdateProductRequest request)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return Result<ProductDetailsResponse>.NotFound("Product not found.");

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
                return Result<ProductDetailsResponse>.NotFound("Category not found.");

            product.CategoryId = request.CategoryId.Value;
        }

        if (request.Name is not null) product.Name = request.Name;
        if (request.ClearDescription) product.Description = null;
        else if (request.Description is not null) product.Description = request.Description;
        if (request.Price.HasValue) product.Price = request.Price.Value;
        if (request.Stock.HasValue) product.Stock = request.Stock.Value;
        if (request.ClearImages) product.Images = null;
        else if (request.Images is not null) product.Images = request.Images;
        if (request.Status.HasValue) product.Status = request.Status.Value;
        if (request.ClearDiscount) product.DiscountPercent = null;
        else if (request.DiscountPercent.HasValue) product.DiscountPercent = request.DiscountPercent;

        product.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        await _notificationServices.CheckLowStock(product.Id);

        var (reviewCount, averageRating) = await GetReviewStats(product.Id);
        var activePromotions = await _promotionServices.GetActivePromotions();

        return Result<ProductDetailsResponse>.Ok(MapToDetails(product, reviewCount, averageRating, activePromotions));
    }

    public async Task<Result<bool>> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
            return Result<bool>.NotFound("Product not found.");

        var hasOrderItems = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
        if (hasOrderItems)
            return Result<bool>.BadRequest("Cannot delete a product that has orders.");

        var hasCartItems = await _context.CartItems.AnyAsync(ci => ci.ProductId == id);
        if (hasCartItems)
            return Result<bool>.BadRequest("Cannot delete a product that is in carts.");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    private static ProductResponse MapToList(Product p, List<Promotion> activePromotions)
    {
        var effectiveDiscount = GetFinalEffectiveDiscount(p, activePromotions);

        return new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            DiscountPercent = effectiveDiscount > 0 ? effectiveDiscount : null,
            FinalPrice = PriceCalculator.GetDiscountedPrice(p.Price, effectiveDiscount),
            Stock = p.Stock,
            Images = p.Images == null ? null : p.Images.Take(1).ToArray(),
            Status = p.Status,
            CategoryName = p.Category.Name
        };
    }

    private static decimal GetFinalEffectiveDiscount(Product p, List<Promotion> activePromotions)
    {
        var baseDiscount = PriceCalculator.GetEffectiveDiscountPercent(p);
        
        var promo = activePromotions.FirstOrDefault(pr => 
            (pr.ProductId == p.Id) || 
            (pr.CategoryId == p.CategoryId && pr.ProductId == null));

        if (promo != null && promo.DiscountPercent > baseDiscount)
            return promo.DiscountPercent;

        return baseDiscount;
    }

    private async Task<(int reviewCount, double? averageRating)> GetReviewStats(int productId)
    {
        var reviewStats = await _context.ProductReviews
            .Where(r => r.ProductId == productId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Count = g.Count(),
                Average = g.Average(r => (double)r.Rating)
            })
            .FirstOrDefaultAsync();

        return (reviewStats?.Count ?? 0, reviewStats?.Average);
    }

    private static ProductDetailsResponse MapToDetails(Product p, int reviewCount, double? averageRating, List<Promotion> activePromotions)
    {
        var effectiveDiscount = GetFinalEffectiveDiscount(p, activePromotions);

        return new ProductDetailsResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            DiscountPercent = p.DiscountPercent,
            EffectiveDiscountPercent = effectiveDiscount > 0 ? effectiveDiscount : null,
            FinalPrice = PriceCalculator.GetDiscountedPrice(p.Price, effectiveDiscount),
            CategoryId = p.CategoryId,
            CategoryName = p.Category.Name,
            Stock = p.Stock,
            Images = p.Images,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            ReviewCount = reviewCount,
            AverageRating = reviewCount > 0 ? Math.Round(averageRating!.Value, 1) : null
        };
    }
}
