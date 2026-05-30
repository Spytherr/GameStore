using System.Security.Claims;

namespace GameStore.api;

public static class GameOffersEndpoints
{
    public static void MapGameOffersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games/{gameId}/offers").WithTags("Game Offers");

        group.MapGet("/", async (int gameId, IGameOffersService service) =>
        {
            var result = await service.GetByGameIdAsync(gameId);
            return result.ToHttpResult();
        });

        group.MapPost("/", async (int gameId, CreateGameOfferDto dto,
            IGameOffersService service, ClaimsPrincipal user) =>
        {
            var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.CreateAsync(gameId, dto, sellerId);
            return result.ToHttpResult();
        }).RequireAuthorization("SellerOnly")
          .AddEndpointFilter<ValidationFilter<CreateGameOfferDto>>();

        group.MapPut("/{offerId}", async (int gameId, int offerId,
            UpdateGameOfferDto dto, IGameOffersService service, ClaimsPrincipal user) =>
        {
            var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.UpdateAsync(gameId, offerId, dto, sellerId);
            return result.ToHttpResult();
        }).RequireAuthorization("SellerOnly")
          .AddEndpointFilter<ValidationFilter<UpdateGameOfferDto>>();

        group.MapDelete("/{offerId}", async (int gameId, int offerId,
            IGameOffersService service, ClaimsPrincipal user) =>
        {
            var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.DeleteAsync(gameId, offerId, sellerId);
            return result.ToHttpResult();
        }).RequireAuthorization("SellerOnly");

        group.MapPost("/{offerId}/discount", async (int gameId, int offerId,
            ApplyOfferDiscountDto dto, IGameOffersService service, ClaimsPrincipal user) =>
        {
            var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.ApplyDiscountAsync(gameId, offerId, dto.DiscountPercentage, sellerId);
            return result.ToHttpResult();
        }).RequireAuthorization("SellerOnly")
          .AddEndpointFilter<ValidationFilter<ApplyOfferDiscountDto>>();

        group.MapDelete("/{offerId}/discount", async (int gameId, int offerId,
            IGameOffersService service, ClaimsPrincipal user) =>
        {
            var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.RemoveDiscountAsync(gameId, offerId, sellerId);
            return result.ToHttpResult();
        }).RequireAuthorization("SellerOnly");
    }
}
