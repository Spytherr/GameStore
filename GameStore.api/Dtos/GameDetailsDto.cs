namespace GameStore.api;

public record GameDetailsDto(
    int Id,
    string Title,
    string? Description,
    int GenreId,
    string GenreName,
    string? ImageUrl,
    DateOnly ReleaseDate,
    List<GameOfferDto> Offers
);
