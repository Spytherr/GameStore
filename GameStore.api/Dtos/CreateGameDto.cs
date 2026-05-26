namespace GameStore.api;

public record CreateGameDto(
    string Title,
    string? Description,
    int GenreId,
    string? ImageUrl,
    DateOnly ReleaseDate
);
