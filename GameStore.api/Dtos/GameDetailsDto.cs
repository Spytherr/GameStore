namespace GameStore.api;

public record GameDetailsDto(
    int Id,
    string Title,
    string? Description,
    int GenreId,
    decimal Price,
    decimal DiscountPercentage,
    decimal? DiscountedPrice,
    bool IsOnSale,
    int Stock,
    string? ImageUrl,
    DateOnly ReleaseDate
);
