namespace GameStore.api;

public class Order
{
    public int Id { get; set; }

    public required string BuyerId { get; set; }
    public ApplicationUser? Buyer { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public string? PaymentTransactionId { get; set; }

    public List<OrderItem> Items { get; set; } = [];
}
