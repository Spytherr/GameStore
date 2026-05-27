namespace GameStore.api;

public record GameOfferDto(
    int Id,
    int GameId,
    string SellerName,
    decimal Price,
    decimal? DiscountedPrice,
    bool IsOnSale,
    int Stock,
    string PlatformName
);
