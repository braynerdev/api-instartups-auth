using Api.Instartups.Auth.Configurations.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Instartups.Auth.Configurations;

public static class MigrationsConfig
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }    
}