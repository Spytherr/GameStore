namespace GameStore.api;

public record CreateOrderDto(
    List<CreateOrderItemDto> Items
);

public record CreateOrderItemDto(
    int GameOfferId,
    int Quantity
);
