using System.Text.Json.Serialization;

namespace GameStore.api;

public record RawgGameDetailsDto(
    int Id,
    string Name,
    string? Released,
    [property: JsonPropertyName("background_image")] string? BackgroundImage,
    List<RawgGenreDto>? Genres,
    double? Rating,
    List<RawgPlatformWrapperDto>? Platforms,
    List<RawgTagDto>? Tags,
    List<RawgDeveloperDto>? Developers,
    List<RawgPublisherDto>? Publishers
);

public record RawgDeveloperDto(int Id, string Name);
public record RawgPublisherDto(int Id, string Name);

public record RawgGenreDto(
    int Id,
    string Name
);
