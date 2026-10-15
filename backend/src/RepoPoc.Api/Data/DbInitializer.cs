using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RepoPoc.Api.Models;

namespace RepoPoc.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));

        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }

        if (await db.Users.AnyAsync())
        {
            return;
        }

        var username = configuration["Seed:AdminUsername"];
        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("No users found and no seed admin credentials configured (Seed:AdminUsername / Seed:AdminPassword).");
            return;
        }

        var admin = new User
        {
            Username = username,
            FirstName = "Admin",
            LastName = "User",
            Email = configuration["Seed:AdminEmail"] ?? "admin@example.com",
            CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = hasher.HashPassword(admin, password);
        db.Users.Add(admin);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded initial admin user '{Username}'.", username);
    }
}
