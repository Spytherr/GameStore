using Microsoft.AspNetCore.HttpOverrides;
using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using GameStore.api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<IGamesService, GamesService>();
builder.Services.AddScoped<IGenresService, GenresService>();
builder.Services.AddScoped<IGameOffersService, GameOffersService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddScoped<IPaymentService, MockPaymentService>();
builder.Services.AddScoped<IRawgService, RawgService>();

builder.Services.AddHttpClient<IRawgService, RawgService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Rawg:BaseUrl"]!);
});

var connectionString = builder.Configuration.GetConnectionString("GameStoreContext");
builder.AddGameStoreDatabase(connectionString);

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<GameStoreContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("SellerOnly", policy => policy.RequireRole("Seller"))
    .AddPolicy("BuyerOnly", policy => policy.RequireRole("Buyer"));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddFixedWindowLimiter("global", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("write", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info ??= new OpenApiInfo();
        document.Info.Title = "GameStore API";
        document.Info.Version = "v1";
        document.Info.Description = "Welcome to the GameStore API!\n\n" +
                                    "Explore all available endpoints below. Some operations require authentication.\n\n" +
                                    "To use protected endpoints, click **Authorize** and paste your JWT token.";

        document.Components ??= new OpenApiComponents();
        var securitySchemes = new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme> 
        { 
            ["Bearer"] = new OpenApiSecurityScheme 
            { 
                Type = SecuritySchemeType.Http, 
                Scheme = "bearer", 
                BearerFormat = "JWT", 
                Description = "Enter your JWT token (e.g. seller token)." 
            } 
        };
        document.Components.SecuritySchemes = securitySchemes;
        return Task.CompletedTask;
    });
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "http://localhost:5174"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseForwardedHeaders();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "GameStore API";
    options.WithTheme(ScalarTheme.DeepSpace);
});

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async httpContext =>
    {
        httpContext.Response.StatusCode = 500;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
    });
});

app.UseRateLimiter();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapGamesEndpoints();
app.MapGenresEndpoints();
app.MapPlatformsEndpoints();
app.MapGameOffersEndpoints();
app.MapOrdersEndpoints();
app.MapRawgEndpoints();

app.MapMethods("/health", new[] { "GET", "HEAD" }, () => Results.Ok(new { status = "healthy" }));

app.MigrateDatabase();
await app.SeedRolesAsync();
await app.ResetDemoDataAsync();


_ = Task.Run(async () =>
{
    try
    {
        await app.SeedGamesFromRawgAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Background seeding error: {ex.Message}");
    }
});

app.Run();

public partial class Program { }
