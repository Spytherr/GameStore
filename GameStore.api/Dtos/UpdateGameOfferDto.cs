using System.ComponentModel.DataAnnotations;

namespace GameStore.api;

public record UpdateGameOfferDto(
    [Required][Range(0.01, 9999.99)] decimal Price,
    [Required][Range(0, 10000)] int Stock
);
