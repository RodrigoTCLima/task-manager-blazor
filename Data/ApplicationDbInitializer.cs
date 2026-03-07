using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;

public static class ApplicationDbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await context.Database.MigrateAsync();

        if (!userManager.Users.Any())
        {
            var user = new IdentityUser 
            { 
                UserName = "admin@test.com", 
                Email = "admin@test.com", 
                EmailConfirmed = true 
            };
            await userManager.CreateAsync(user, "Admin123!");
        }
    }
}
