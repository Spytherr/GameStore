using System.Text.Json.Serialization;

namespace GameStore.api;

public record RawgSearchResponseDto(
    int Count,
    List<RawgGameSearchResultDto> Results
);

public record RawgGameSearchResultDto(
    int Id,
    string Name,
    string? Released,
    [property: JsonPropertyName("background_image")] string? BackgroundImage,
    List<RawgGenreDto>? Genres,
    double? Rating
);
