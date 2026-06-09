using E_Commerce.Models;

namespace E_Commerce.Common.Pricing;

public static class PriceCalculator
{
    public static decimal GetEffectiveDiscountPercent(Product product)
    {
        if (product.DiscountPercent.HasValue)
            return product.DiscountPercent.Value;

        return product.Category?.DiscountPercent ?? 0m;
    }

    public static decimal GetDiscountedPrice(decimal basePrice, decimal discountPercent)
    {
        if (discountPercent <= 0)
            return basePrice;

        return Math.Round(basePrice * (1 - discountPercent / 100m), 2);
    }
}
