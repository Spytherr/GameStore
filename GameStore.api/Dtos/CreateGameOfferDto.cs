namespace GameStore.api;

public record CreateGameOfferDto(
    decimal Price,
    int Stock,
    int PlatformId
);
