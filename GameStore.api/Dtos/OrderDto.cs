namespace GameStore.api;

public record OrderDto(
    int Id,
    DateTime OrderDate,
    string Status,
    decimal TotalAmount,
    string? PaymentTransactionId,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    int Id,
    string GameTitle,
    string SellerName,
    string PlatformName,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal
);
