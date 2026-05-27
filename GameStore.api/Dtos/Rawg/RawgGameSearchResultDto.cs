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
    double? Rating,
    List<RawgPlatformWrapperDto>? Platforms,
    List<RawgTagDto>? Tags
);

public record RawgPlatformWrapperDto(RawgPlatformDto Platform);
public record RawgPlatformDto(int Id, string Name);
public record RawgTagDto(int Id, string Name);
