using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TaskManager.Data;

/// <summary>
/// Used only by EF Core CLI tools (dotnet ef migrations add, dotnet ef database update).
/// Never runs in production. Allows generating PostgreSQL migrations locally.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Use PostgreSQL for migration generation
        // Replace with your actual Neon connection string when running migrations
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "Host=localhost;Database=taskmanager_dev;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
