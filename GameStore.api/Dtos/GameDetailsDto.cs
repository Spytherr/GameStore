namespace GameStore.api;

public record GameDetailsDto(
    int Id,
    string Title,
    int GenreId,
    decimal Price,
    DateOnly ReleaseDate
);
