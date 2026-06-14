using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;

namespace TaskManager.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationMember> OrganizationMembers { get; set; }
    public DbSet<OrganizationInvite> OrganizationInvites { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var isPostgres = Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

        // Comment -> TaskItem
        builder.Entity<Comment>()
            .HasOne<TaskItem>()
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Default values for JSON columns — syntax differs between SQLite and PostgreSQL
        var emptyJson = isPostgres ? "'[]'::text" : "'[]'";

        builder.Entity<TaskItem>()
            .Property(t => t.Tags)
            .HasDefaultValueSql(emptyJson);

        builder.Entity<TaskItem>()
            .Property(t => t.DependencyOnTaskIds)
            .HasDefaultValueSql(emptyJson);

        builder.Entity<TaskItem>()
            .Property(t => t.ReviewByUserId)
            .HasDefaultValueSql(emptyJson);

        builder.Entity<TaskItem>()
            .Property(t => t.ReviewedByUserId)
            .HasDefaultValueSql(emptyJson);

        // OrganizationMember -> Organization
        builder.Entity<OrganizationMember>()
            .HasOne(m => m.Organization)
            .WithMany(o => o.Members)
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrganizationInvite -> Organization
        builder.Entity<OrganizationInvite>()
            .HasOne(i => i.Organization)
            .WithMany(o => o.Invites)
            .HasForeignKey(i => i.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        // JSON serialization for List<string> and List<int> fields
        // Same approach works for both SQLite and PostgreSQL (stored as text)
        var jsonOptions = (System.Text.Json.JsonSerializerOptions?)null;

        builder.Entity<TaskItem>()
            .Property(t => t.Tags)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, jsonOptions),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
            );

        builder.Entity<TaskItem>()
            .Property(t => t.AssignedToUserIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, jsonOptions),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
            );

        builder.Entity<TaskItem>()
            .Property(t => t.DependencyOnTaskIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v ?? new List<int>(), jsonOptions),
                v => System.Text.Json.JsonSerializer.Deserialize<List<int>>(v, jsonOptions) ?? new List<int>()
            );

        builder.Entity<TaskItem>()
            .Property(t => t.ReviewByUserId)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v ?? new List<string>(), jsonOptions),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
            );

        builder.Entity<TaskItem>()
            .Property(t => t.ReviewedByUserId)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v ?? new List<string>(), jsonOptions),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
            );
    }
}
