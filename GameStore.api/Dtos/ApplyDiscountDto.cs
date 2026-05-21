using System.ComponentModel.DataAnnotations;

namespace GameStore.api;

public record ApplyDiscountDto(
    [Required][Range(1, 90)] decimal DiscountPercentage
);
