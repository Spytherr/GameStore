using System.Security.Claims;

namespace GameStore.api;

public static class GameOffersEndpoints
{
    public static void MapGameOffersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games/{gameId}/offers");

        // GET /games/{gameId}/offers
        group.MapGet("/", async (int gameId, IGameOffersService service) =>
        {
            var result = await service.GetByGameIdAsync(gameId);
            return result.ToHttpResult();
        });

        // POST /games/{gameId}/offers — Seller only
        group.MapPost("/", async (int gameId, CreateGameOfferDto dto,
            IGameOffersService service, ClaimsPrincipal user) =>
        {
            var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.CreateAsync(gameId, dto, sellerId);
            return result.ToHttpResult();
        }).RequireAuthorization("SellerOnly");

        // PUT /games/{gameId}/offers/{offerId} — Seller only
        group.MapPut("/{offerId}", async (int gameId, int offerId,
            UpdateGameOfferDto dto, IGameOffersService service, ClaimsPrincipal user) =>
        {
            var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.UpdateAsync(offerId, dto, sellerId);
            return result.ToHttpResult();
        }).RequireAuthorization("SellerOnly");

        // DELETE /games/{gameId}/offers/{offerId} — Seller only
        group.MapDelete("/{offerId}", async (int gameId, int offerId,
            IGameOffersService service, ClaimsPrincipal user) =>
        {
            var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.DeleteAsync(offerId, sellerId);
            return result.ToHttpResult();
        }).RequireAuthorization("SellerOnly");

        // POST /games/{gameId}/offers/{offerId}/discount — Seller only
        group.MapPost("/{offerId}/discount", async (int gameId, int offerId,
            ApplyOfferDiscountDto dto, IGameOffersService service, ClaimsPrincipal user) =>
        {
            var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.ApplyDiscountAsync(offerId, dto.DiscountPercentage, sellerId);
            return result.ToHttpResult();
        }).RequireAuthorization("SellerOnly");

        // DELETE /games/{gameId}/offers/{offerId}/discount — Seller
        group.MapDelete("/{offerId}/discount", async (int gameId, int offerId,
            IGameOffersService service, ClaimsPrincipal user) =>
        {
            var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.RemoveDiscountAsync(offerId, sellerId);
            return result.ToHttpResult();
        }).RequireAuthorization("SellerOnly");
    }
}
