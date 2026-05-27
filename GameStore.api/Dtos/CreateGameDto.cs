namespace GameStore.api;

public record CreateGameDto(
    string Title,
    string? Description,
    List<int> GenreIds,
    List<int> PlatformIds,
    string? ImageUrl,
    DateOnly ReleaseDate
);
