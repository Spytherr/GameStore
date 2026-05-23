namespace GameStore.api;

public record GameSummaryDto(
    int Id,
    string Title,
    string Genre,
    string? ImageUrl,
    decimal? LowestPrice,
    bool HasOffersOnSale,
    int TotalOffers,
    DateOnly ReleaseDate
);
