namespace GameStore.api;

public record AuthResponseDto(
    string Token,
    string Email,
    string DisplayName,
    string Role,
    DateTime Expiration
);
