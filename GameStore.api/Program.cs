using GameStore.api;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();

var connectionString = builder.Configuration.GetConnectionString("GameStoreContext");
builder.AddGameStoreDatabase(connectionString);

var app = builder.Build();

app.MapGamesEndpoints();
app.MapGenresEndpoints();

app.MigrateDatabase(); 

app.Run();
