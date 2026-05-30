namespace GameStore.api;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int GameOfferId { get; set; }
    public GameOffer? GameOffer { get; set; }

    public required string GameTitle { get; set; }
    public required string SellerName { get; set; }
    public required string PlatformName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
