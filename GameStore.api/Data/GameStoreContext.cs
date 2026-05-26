using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public class GameStoreContext(DbContextOptions<GameStoreContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<GameOffer> GameOffers => Set<GameOffer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>(entity =>
        {
            entity.Property(g => g.Description)
                .HasMaxLength(1000);

            entity.Property(g => g.ImageUrl)
                .HasMaxLength(500);

            entity.HasIndex(g => g.Title)
                .IsUnique();

            entity.HasIndex(g => g.RawgId)
                .IsUnique()
                .HasFilter("[RawgId] IS NOT NULL");
        });

        modelBuilder.Entity<GameOffer>(entity =>
        {
            entity.Property(o => o.Price)
                .HasPrecision(18, 2);

            entity.Property(o => o.DiscountPercentage)
                .HasPrecision(5, 2);

            entity.Property(o => o.Stock)
                .HasDefaultValue(0);

            entity.HasIndex(o => new { o.GameId, o.SellerId })
                .IsUnique();

            entity.HasOne(o => o.Game)
                .WithMany(g => g.Offers)
                .HasForeignKey(o => o.GameId);

            entity.HasOne(o => o.Seller)
                .WithMany(u => u.GameOffers)
                .HasForeignKey(o => o.SellerId);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            entity.Property(o => o.Status)
                .HasConversion<string>();

            entity.HasOne(o => o.Buyer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            entity.HasOne(oi => oi.GameOffer)
                .WithMany()
                .HasForeignKey(oi => oi.GameOfferId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
