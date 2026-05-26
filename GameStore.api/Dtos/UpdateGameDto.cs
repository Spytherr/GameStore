namespace GameStore.api;

public record UpdateGameDto(
    string Title,
    string? Description,
    int GenreId,
    string? ImageUrl,
    DateOnly ReleaseDate
);
