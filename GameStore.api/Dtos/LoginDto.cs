using System.ComponentModel.DataAnnotations;

namespace GameStore.api;

public record LoginDto(
    [Required][EmailAddress] string Email,
    [Required] string Password
);
