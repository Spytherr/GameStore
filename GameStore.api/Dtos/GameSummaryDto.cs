namespace GameStore.api;

public record GameSummaryDto(
    int Id,
    string Title,
    List<string> Genres,
    List<string> Platforms,
    string? ImageUrl,
    decimal? LowestPrice,
    bool HasOffersOnSale,
    int TotalOffers,
    double? Rating,
    DateOnly ReleaseDate
);
