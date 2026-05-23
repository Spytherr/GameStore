using System.ComponentModel.DataAnnotations;

namespace GameStore.api;

public record ApplyOfferDiscountDto(
    [Required][Range(1, 90)] decimal DiscountPercentage
);
