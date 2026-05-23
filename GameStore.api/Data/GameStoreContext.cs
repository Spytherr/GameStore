using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public class GameStoreContext(DbContextOptions<GameStoreContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<GameOffer> GameOffers => Set<GameOffer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>(entity =>
        {
            entity.Property(g => g.Description)
                .HasMaxLength(1000);

            entity.Property(g => g.ImageUrl)
                .HasMaxLength(500);
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
    }
}
