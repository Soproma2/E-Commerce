using E_Commerce.DTOs.Requests;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace E_Commerce.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<AdminNotification> AdminNotifications => Set<AdminNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var stringArrayComparer = new ValueComparer<string[]?>(
            (left, right) => left == null
                ? right == null
                : right != null && left.SequenceEqual(right),
            value => value == null
                ? 0
                : value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item == null ? 0 : item.GetHashCode())),
            value => value == null ? null : value.ToArray());

        // ─── User ───────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(u => u.Id);

            e.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            e.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(u => u.PasswordHash)
                .IsRequired();

            e.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20);

            e.Property(u => u.Balance)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(1000m);

            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();

            e.HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Category ───────────────────────────────────────────
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("Categories");
            e.HasKey(c => c.Id);

            e.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            e.HasOne(c => c.Parent)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(c => c.DiscountPercent)
                .HasColumnType("decimal(5,2)");
        });

        // ─── Product ─────────────────────────────────────────────
        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("Products");
            e.HasKey(p => p.Id);

            e.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            e.Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            e.Property(p => p.DiscountPercent)
                .HasColumnType("decimal(5,2)");

            e.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            e.Property(p => p.Images)
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => v == null ? null : JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions?)null)
                )
                .Metadata.SetValueComparer(stringArrayComparer);
        });

        // ─── Order ───────────────────────────────────────────────
        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasKey(o => o.Id);

            e.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            e.Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            e.Property(o => o.ShippingAddress)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<ShippingAddressDto>(v, (JsonSerializerOptions?)null)!
                )
                .HasColumnType("nvarchar(max)");

            e.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── OrderItem ───────────────────────────────────────────
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItems");
            e.HasKey(oi => oi.Id);

            e.Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");

            e.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Cart ────────────────────────────────────────────────
        modelBuilder.Entity<Cart>(e =>
        {
            e.ToTable("Carts");
            e.HasKey(c => c.Id);

            e.HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── CartItem ────────────────────────────────────────────
        modelBuilder.Entity<CartItem>(e =>
        {
            e.ToTable("CartItems");
            e.HasKey(ci => ci.Id);

            e.HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── ProductReview ───────────────────────────────────────
        modelBuilder.Entity<ProductReview>(e =>
        {
            e.ToTable("ProductReviews");
            e.HasKey(r => r.Id);

            e.Property(r => r.Rating)
                .IsRequired();

            e.Property(r => r.Comment)
                .IsRequired()
                .HasMaxLength(2000);

            e.HasIndex(r => new { r.UserId, r.ProductId })
                .IsUnique();

            e.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── WishlistItem ───────────────────────────────────────
        modelBuilder.Entity<WishlistItem>(e =>
        {
            e.ToTable("WishlistItems");
            e.HasKey(w => w.Id);

            e.HasIndex(w => new { w.UserId, w.ProductId })
                .IsUnique();

            e.HasOne(w => w.User)
                .WithMany(u => u.Wishlist)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(w => w.Product)
                .WithMany(p => p.WishlistItems)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── UserAddress ────────────────────────────────────────
        modelBuilder.Entity<UserAddress>(e =>
        {
            e.ToTable("UserAddresses");
            e.HasKey(a => a.Id);

            e.Property(a => a.FullName).IsRequired().HasMaxLength(100);
            e.Property(a => a.Street).IsRequired().HasMaxLength(200);
            e.Property(a => a.City).IsRequired().HasMaxLength(100);
            e.Property(a => a.Country).IsRequired().HasMaxLength(100);
            e.Property(a => a.ZipCode).IsRequired().HasMaxLength(20);

            e.HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Promotion ──────────────────────────────────────────
        modelBuilder.Entity<Promotion>(e =>
        {
            e.ToTable("Promotions");
            e.HasKey(p => p.Id);

            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.Property(p => p.DiscountPercent).HasColumnType("decimal(5,2)");

            e.HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ─── AdminNotification ──────────────────────────────────
        modelBuilder.Entity<AdminNotification>(e =>
        {
            e.ToTable("AdminNotifications");
            e.HasKey(n => n.Id);

            e.Property(n => n.Message).IsRequired().HasMaxLength(500);
            e.Property(n => n.Type).HasMaxLength(50);
        });
    }
}
