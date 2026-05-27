namespace GameStore.api;

public record UpdateGameDto(
    string Title,
    string? Creators,
    string? Publishers,
    List<int> GenreIds,
    List<int> PlatformIds,
    string? ImageUrl,
    DateOnly ReleaseDate
);
