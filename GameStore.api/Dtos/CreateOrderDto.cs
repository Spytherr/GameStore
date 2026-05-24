using System.ComponentModel.DataAnnotations;

namespace GameStore.api;

public record CreateOrderDto(
    [Required] List<CreateOrderItemDto> Items
);

public record CreateOrderItemDto(
    [Required] int GameOfferId,
    [Required][Range(1, 100)] int Quantity
);
