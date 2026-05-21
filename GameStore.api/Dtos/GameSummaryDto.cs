namespace GameStore.api;

public record GameSummaryDto(
    int Id,
    string Title,
    string Genre,
    decimal Price,
    decimal? DiscountedPrice,
    bool IsOnSale,
    DateOnly ReleaseDate
);
