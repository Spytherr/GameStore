using System.ComponentModel.DataAnnotations;

namespace GameStore.api;

public record UpdateGameDto(
    [Required][StringLength(50)] string Title,
    [StringLength(1000)] string? Description,
    [Required][Range(1, 100)] int GenreId,
    [StringLength(500)] string? ImageUrl,
    [Required] DateOnly ReleaseDate
);
