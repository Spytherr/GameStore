using System.ComponentModel.DataAnnotations;

namespace GameStore.api;

public record RegisterDto(
    [Required][EmailAddress] string Email,
    [Required][StringLength(50, MinimumLength = 2)] string DisplayName,
    [Required][StringLength(100, MinimumLength = 6)] string Password,
    [Required] string Role
);
