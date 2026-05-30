namespace GameStore.api;

public record AuthResponseDto(
    string Token,
    string UserId,
    string Email,
    string DisplayName,
    string Role,
    DateTime Expiration
);
