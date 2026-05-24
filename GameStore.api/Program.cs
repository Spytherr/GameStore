using System.Text;
using GameStore.api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();

builder.Services.AddScoped<IGamesService, GamesService>();
builder.Services.AddScoped<IGenresService, GenresService>();
builder.Services.AddScoped<IGameOffersService, GameOffersService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddScoped<IPaymentService, MockPaymentService>();

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

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapGamesEndpoints();
app.MapGenresEndpoints();
app.MapGameOffersEndpoints();
app.MapOrdersEndpoints();

app.MigrateDatabase();
await app.SeedRolesAsync();

app.Run();
