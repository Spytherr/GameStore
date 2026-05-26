using System.Security.Claims;

namespace GameStore.api;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/orders")
            .RequireAuthorization("BuyerOnly")
            .WithTags("Orders");

        group.MapPost("/", async (CreateOrderDto dto, IOrdersService service, ClaimsPrincipal user) =>
        {
            var buyerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.CreateAsync(dto, buyerId);
            return result.ToHttpResult();
        }).AddEndpointFilter<ValidationFilter<CreateOrderDto>>();

        group.MapGet("/", async (IOrdersService service, ClaimsPrincipal user) =>
        {
            var buyerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.GetByBuyerAsync(buyerId);
            return result.ToHttpResult();
        });

        group.MapGet("/{id}", async (int id, IOrdersService service, ClaimsPrincipal user) =>
        {
            var buyerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.GetByIdAsync(id, buyerId);
            return result.ToHttpResult();
        });

        group.MapPost("/{id}/cancel", async (int id, IOrdersService service, ClaimsPrincipal user) =>
        {
            var buyerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await service.CancelAsync(id, buyerId);
            return result.ToHttpResult();
        });
    }
}
