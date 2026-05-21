using System;
using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public static class DataExtensions
{
    public static void MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GameStoreContext>();
        context.Database.Migrate();
    }

    public static void AddGameStoreDatabase(this WebApplicationBuilder builder, string connectionString)
    {
        builder.Services.AddSqlServer<GameStoreContext>(
            connectionString,
            optionsAction: options =>
            {
                options.UseSeeding((context, _) =>
                {
                    if (!context.Set<Genre>().Any())
                    {
                        context.Set<Genre>().AddRange(
                            new Genre { Name = "Action" },
                            new Genre { Name = "Adventure" },
                            new Genre { Name = "RPG" },
                            new Genre { Name = "Strategy" },
                            new Genre { Name = "Sports" }
                        );
                    }
                    context.SaveChanges();
                });
            }
        );
    }
}
