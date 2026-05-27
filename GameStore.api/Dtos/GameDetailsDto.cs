namespace GameStore.api;

public record GameDetailsDto(
    int Id,
    string Title,
    string? Description,
    List<string> Genres,
    List<string> Platforms,
    string? ImageUrl,
    DateOnly ReleaseDate,
    double? Rating,
    List<GameOfferDto> Offers
);
