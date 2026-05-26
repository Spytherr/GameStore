namespace GameStore.api;

public record RegisterDto(
    string Email,
    string DisplayName,
    string Password,
    string Role
);
