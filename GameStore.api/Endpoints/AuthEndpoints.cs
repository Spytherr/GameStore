namespace GameStore.api;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth").RequireRateLimiting("auth");

        group.MapPost("/register", async (RegisterDto dto, IAuthService service) =>
        {
            var result = await service.RegisterAsync(dto);
            return result.ToHttpResult();
        }).AddEndpointFilter<ValidationFilter<RegisterDto>>();

        group.MapPost("/login", async (LoginDto dto, IAuthService service) =>
        {
            var result = await service.LoginAsync(dto);
            return result.ToHttpResult();
        }).AddEndpointFilter<ValidationFilter<LoginDto>>();
    }
}
