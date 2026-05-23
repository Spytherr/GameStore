namespace GameStore.api;

public class GameOffer
{
    public int Id { get; set; }

    public int GameId { get; set; }
    public Game? Game { get; set; }

    public required string SellerId { get; set; }
    public ApplicationUser? Seller { get; set; }

    public decimal Price { get; set; }
    public int Stock { get; set; }
    public decimal DiscountPercentage { get; set; }
    public bool IsOnSale { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
