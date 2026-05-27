namespace GameStore.api;

public record UpdateGameDto(
    string Title,
    string? Description,
    List<int> GenreIds,
    List<int> PlatformIds,
    string? ImageUrl,
    DateOnly ReleaseDate
);
