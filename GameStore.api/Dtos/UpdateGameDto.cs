using System.ComponentModel.DataAnnotations;

namespace GameStore.api;

public record UpdateGameDto(
    [Required][StringLength(50)] string Title,
    [Required][Range(1, 100)] int GenreId,
    [Required][Range(0, 999.99)] decimal Price,
    [Required] DateOnly ReleaseDate
);
