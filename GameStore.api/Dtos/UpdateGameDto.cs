using System.ComponentModel.DataAnnotations;

namespace GameStore.api;

public record UpdateGameDto(
    [Required][StringLength(50)] string Title,
    [StringLength(1000)] string? Description,
    [Required][Range(1, 100)] int GenreId,
    [Required][Range(0, 999.99)] decimal Price,
    [Required][Range(0, 10000)] int Stock,
    [StringLength(500)] string? ImageUrl,
    [Required] DateOnly ReleaseDate
);
