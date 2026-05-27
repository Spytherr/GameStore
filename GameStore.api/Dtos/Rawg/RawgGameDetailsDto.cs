using System.Text.Json.Serialization;

namespace GameStore.api;

public record RawgGameDetailsDto(
    int Id,
    string Name,
    [property: JsonPropertyName("description_raw")] string? DescriptionRaw,
    string? Released,
    [property: JsonPropertyName("background_image")] string? BackgroundImage,
    List<RawgGenreDto>? Genres,
    double? Rating,
    List<RawgPlatformWrapperDto>? Platforms,
    List<RawgTagDto>? Tags
);

public record RawgGenreDto(
    int Id,
    string Name
);
