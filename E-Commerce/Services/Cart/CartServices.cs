using E_Commerce.Common.Pricing;
using E_Commerce.Common.Results;
using E_Commerce.Data;
using E_Commerce.DTOs.Requests;
using E_Commerce.DTOs.Responses;
using E_Commerce.Enums;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services.Cart;

public class CartServices : ICartServices
{
    private readonly DataContext _context;

    public CartServices(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<CartResponse>> GetCart(int userId)
    {
        var cart = await GetOrCreateCart(userId);
        return Result<CartResponse>.Ok(MapToResponse(cart));
    }

    public async Task<Result<CartResponse>> AddToCart(int userId, AddToCartRequest request)
    {
        var product = await _context.Products.FindAsync(request.ProductId);

        if (product is null)
            return Result<CartResponse>.NotFound("Product not found.");

        if (product.Status != ProductStatus.Active)
            return Result<CartResponse>.BadRequest("Product is not available.");

        var cart = await GetOrCreateCart(userId);

        var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
        var requestedTotalQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;

        if (product.Stock < requestedTotalQuantity)
            return Result<CartResponse>.BadRequest("Insufficient stock.");

        if (existingItem is not null)
        {
            existingItem.Quantity = requestedTotalQuantity;
        }
        else
        {
            cart.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });
        }

        cart.UpdateAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _context.Entry(cart).Collection(c => c.CartItems).LoadAsync();
        foreach (var item in cart.CartItems)
        {
            await _context.Entry(item).Reference(ci => ci.Product).LoadAsync();
            await _context.Entry(item.Product).Reference(p => p.Category).LoadAsync();
        }

        return Result<CartResponse>.Ok(MapToResponse(cart));
    }

    public async Task<Result<CartResponse>> EditCartItem(int userId, int cartItemId, EditCartItemRequest request)
    {
        var cart = await GetCartWithItems(userId);

        if (cart is null)
            return Result<CartResponse>.NotFound("Cart not found.");

        var item = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);

        if (item is null)
            return Result<CartResponse>.NotFound("Cart item not found.");

        if (request.Quantity <= 0)
        {
            cart.CartItems.Remove(item);
            _context.CartItems.Remove(item);
        }
        else
        {
            if (item.Product.Status != ProductStatus.Active)
                return Result<CartResponse>.BadRequest("Product is not available.");

            if (item.Product.Stock < request.Quantity)
                return Result<CartResponse>.BadRequest("Insufficient stock.");

            item.Quantity = request.Quantity;
        }

        cart.UpdateAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Result<CartResponse>.Ok(MapToResponse(cart));
    }

    public async Task<Result<bool>> DeleteCartItem(int userId, int cartItemId)
    {
        var cart = await GetCartWithItems(userId);

        if (cart is null)
            return Result<bool>.NotFound("Cart not found.");

        var item = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);

        if (item is null)
            return Result<bool>.NotFound("Cart item not found.");

        _context.CartItems.Remove(item);
        cart.UpdateAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    private async Task<E_Commerce.Models.Cart> GetOrCreateCart(int userId)
    {
        var cart = await GetCartWithItems(userId);

        if (cart is not null)
            return cart;

        var newCart = new E_Commerce.Models.Cart { UserId = userId };
        _context.Carts.Add(newCart);
        await _context.SaveChangesAsync();

        return newCart;
    }

    private async Task<E_Commerce.Models.Cart?> GetCartWithItems(int userId)
    {
        return await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    private static CartResponse MapToResponse(E_Commerce.Models.Cart c) => new()
    {
        Id = c.Id,
        UserId = c.UserId,
        CartItems = c.CartItems.Select(ci =>
        {
            var discount = PriceCalculator.GetEffectiveDiscountPercent(ci.Product);
            var unitPrice = PriceCalculator.GetDiscountedPrice(ci.Product.Price, discount);

            return new CartItemResponse
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                ProductImages = ci.Product.Images,
                UnitPrice = unitPrice,
                Quantity = ci.Quantity
            };
        }).ToList()
    };
}
