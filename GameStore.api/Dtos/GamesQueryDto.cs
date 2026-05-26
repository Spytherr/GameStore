namespace GameStore.api;

public record GamesQueryDto(
    int Page = 1,
    int PageSize = 10,
    int? GenreId = null,
    string? Search = null,
    string SortBy = "title",
    string SortOrder = "asc"
);
